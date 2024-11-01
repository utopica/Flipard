const quizState = {
    cards: [],
    currentIndex: 0,
    showAnswer: false,
    startTime: Date.now(),
    quizAnswers: [],
    correctAnswersCount: 0,
    blankAnswersCount: 0,
    wrongAnswerCount: 0,
    originalAnswers: [],
    deckId: '',
    quizResultsUrl: '',
    answeredQuestions: [],
    isMenuCollapsed: false,
    isQuizCompleted: false
};

function initializeQuiz(quizCards, deck, resultsUrl) {
    
    const urlParams = new URLSearchParams(window.location.search);
    const settingsParam = urlParams.get('settings');

    let settings = {
        modes: { feedback: true },
        questionCount: quizCards.length,
        answerWith: 'term',
        questionTypes: {
            written: true,
            multipleChoice: true,
            trueFalse: false
        }
    };

    if (settingsParam) {
        try {
            const parsedSettings = JSON.parse(settingsParam);
            settings = { ...settings, ...parsedSettings };
        } catch (e) {
            console.error('Failed to parse quiz settings:', e);
        }
    }

    quizState.cards = settings.questionCount < quizCards.length ?
        shuffleArray(quizCards).slice(0, settings.questionCount) :
        quizCards;
    
    quizState.cards = quizCards;
    quizState.deckId = deck;
    quizState.quizResultsUrl = resultsUrl;
    quizState.originalAnswers = new Array(quizCards.length).fill('');
    quizState.startTime = Date.now();
    quizState.isQuizCompleted = false;
    quizState.feedbackMode = settings.modes.feedback;
    quizState.answerWith = settings.answerWith;
    quizState.questionTypes = settings.questionTypes;

    if (settings.answerWith === 'definition') {
        quizState.cards = quizState.cards.map(card => ({
            ...card,
            Term: card.Meaning,
            Meaning: card.Term
        }));
    }

    showCard(quizState.currentIndex);
    initializeQuestionsMenu();
}

function shuffleArray(array) {
    const newArray = [...array];
    for (let i = newArray.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [newArray[i], newArray[j]] = [newArray[j], newArray[i]];
    }
    return newArray;
}

function initializeQuestionsMenu() {
    const grid = document.getElementById('questionsGrid');
    if (!grid) return;

    grid.innerHTML = '';

    for (let i = 0; i < quizState.cards.length; i++) {
        const button = document.createElement('button');
        button.className = getQuestionButtonClass(i);
        button.textContent = (i + 1).toString();

        const isAnswered = quizState.answeredQuestions.some(q => q.questionIndex === i);
        button.disabled = !isAnswered && i !== quizState.currentIndex; //user cevaplanmamış sorulara gidemez soru menüsünü kullanarak

        button.onclick = () => {
            if (!button.disabled) {
                showCard(i);
            }
        };

        grid.appendChild(button);
    }
}

function getQuestionButtonClass(index) {
    const baseClasses = ['question-button'];

    if (index === quizState.currentIndex) {
        baseClasses.push('current');
    }

    const answer = quizState.answeredQuestions.find(a => a.questionIndex === index);
    if (answer) {
        if (answer.isCorrect) {
            baseClasses.push('correct');
        } else if (answer.userAnswer === '') {
            baseClasses.push('blank');
        } else {
            baseClasses.push('incorrect');
        }
    }

    return baseClasses.join(' ');
}

function toggleMenu() {
    const menu = document.getElementById('questionsMenu');
    if (menu) {
        quizState.isMenuCollapsed = !quizState.isMenuCollapsed;
        menu.classList.toggle('collapsed', quizState.isMenuCollapsed);
    }
}

function showCard(index) {
    if (index >= 0 && index < quizState.cards.length) {
        quizState.currentIndex = index;
        quizState.showAnswer = false;
        
        initializeQuestionsMenu();

        const meaningText = document.getElementById("card-meaning-text");
        if (meaningText) {
            meaningText.innerText = quizState.cards[index].Meaning;
        }

        const termInputContainer = document.getElementById("card-term-container");
        const feedbackContainer = document.getElementById("feedback-container");
        const userAnswerDisplay = document.querySelector(".user-answer-display");
        const answerFeedback = document.getElementById("answer-feedback");

        // Find if this question was previously answered
        const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === index);

        // Handle input and feedback display
        const termInput = document.getElementById("term-input");
        if (termInput) {
            termInput.value = previousAnswer ? previousAnswer.userAnswer : '';
            termInput.disabled = quizState.isQuizCompleted;
        }

        if (previousAnswer && !previousAnswer.isCorrect && quizState.feedbackMode) {
            // If there's a previous wrong answer and we're in feedback mode,
            // show the feedback display
            termInputContainer.style.display = 'none';
            feedbackContainer.style.display = 'block';

            // Show user's previous answer
            const isBlank = previousAnswer.userAnswer === '';
            userAnswerDisplay.innerHTML = `
                <div class="wrong-answer">
                    ${isBlank ? 'Boş bırakıldı' : previousAnswer.userAnswer}
                    <i class="fi fi-rr-cross-small"></i>
                </div>`;

            // Show correct answer
            answerFeedback.innerHTML = `
                <div class="correct-answer">
                    ${quizState.cards[index].Term}
                    <i class="fi fi-br-check"></i>
                </div>`;

            quizState.showAnswer = true;
        } else {
            // Otherwise, show the input container
            termInputContainer.style.display = 'block';
            feedbackContainer.style.display = 'none';
            if (userAnswerDisplay) userAnswerDisplay.innerHTML = '';
            if (answerFeedback) answerFeedback.innerHTML = '';
        }
        
        const progress = document.querySelector(".card-heading #progress");
        if (progress) {
            progress.innerText = `${index + 1}/${quizState.cards.length}`;
        }
        
        const imageElement = document.getElementById("meaning-image");
        const imageContainer = document.getElementById("card-meaning-image");

        if (quizState.cards[index].ImageUrl) {
            if (!imageElement && imageContainer) {
                const img = document.createElement("img");
                img.id = "meaning-image";
                img.src = quizState.cards[index].ImageUrl;
                img.alt = "Image";
                imageContainer.appendChild(img);
            } else if (imageElement) {
                imageElement.src = quizState.cards[index].ImageUrl;
                imageElement.style.display = "block";
            }
        } else if (imageElement) {
            imageElement.style.display = "none";
        }

        quizState.showAnswer = false; //
    }
}

document.addEventListener("DOMContentLoaded", function () {
    showCard(quizState.currentIndex);
    initializeQuestionsMenu();
    quizState.startTime = Date.now();
});

function showPreviousCard() {
    if (quizState.currentIndex > 0) {
        showCard(quizState.currentIndex - 1);
    }
}

function submitAnswer() {
    const termInputContainer = document.getElementById("card-term-container");
    const feedbackContainer = document.getElementById("feedback-container");
    const userAnswerDisplay = document.querySelector(".user-answer-display");
    const termInput = document.getElementById("term-input");
    const userTerm = termInput.value.trim();
    const correctTerm = quizState.cards[quizState.currentIndex].Term;
    const answerFeedback = document.getElementById("answer-feedback");
    const isBlank = userTerm === '';
    const isCorrect = !isBlank && (userTerm.toLowerCase() === correctTerm.toLowerCase());
    const existingAnswerIndex = quizState.answeredQuestions.findIndex(
        q => q.questionIndex === quizState.currentIndex
    );


    if (existingAnswerIndex !== -1 && !quizState.feedbackMode) {
        const oldAnswer = quizState.answeredQuestions[existingAnswerIndex];

        // Revert old answer statistics
        if (oldAnswer.isBlank) quizState.blankAnswersCount--;
        else if (oldAnswer.isCorrect) quizState.correctAnswersCount--;
        else quizState.wrongAnswerCount--;

        // Remove old answer
        quizState.answeredQuestions.splice(existingAnswerIndex, 1);
        quizState.originalAnswers[quizState.currentIndex] = '';
    }

    if (!quizState.originalAnswers[quizState.currentIndex] || !quizState.feedbackMode) {
        quizState.originalAnswers[quizState.currentIndex] = userTerm;

        if (!quizState.showAnswer) {
            quizState.answeredQuestions.push({
                questionIndex: quizState.currentIndex,
                userAnswer: userTerm,
                isCorrect: isCorrect,
                isBlank: isBlank
            });

            // Update statistics for new answer
            if (isBlank) quizState.blankAnswersCount++;
            else if (isCorrect) quizState.correctAnswersCount++;
            else quizState.wrongAnswerCount++;
        }
    }

    if (quizState.feedbackMode && !quizState.showAnswer) {
        if (!isCorrect) {
            termInputContainer.style.display = 'none';
            feedbackContainer.style.display = 'block';

            userAnswerDisplay.innerHTML = `
                <div class="wrong-answer">
                    ${isBlank ? 'Boş bırakıldı' : userTerm} 
                    <i class="fi fi-rr-cross-small"></i>
                </div>`;

            // Show correct answer feedback
            answerFeedback.innerHTML = `
                <div class="correct-answer">
                    ${correctTerm} 
                    <i class="fi fi-br-check"></i>
                </div>`;

            quizState.showAnswer = true;
            initializeQuestionsMenu();
            return;
        }
    }

    if (quizState.showAnswer) {
        
        // Reset display for next question
        termInputContainer.style.display = 'block';
        feedbackContainer.style.display = 'none';
        userAnswerDisplay.innerHTML = '';
        answerFeedback.innerHTML = '';
        
        quizState.showAnswer = false;
        if (quizState.currentIndex < quizState.cards.length - 1) {
            showCard(quizState.currentIndex + 1);
        } else {
            showQuizSummary();
        }
        return;
    }

    if (!quizState.feedbackMode || isCorrect) {
        if (quizState.currentIndex < quizState.cards.length - 1) {
            showCard(quizState.currentIndex + 1);
        } else {
            showQuizSummary();
        }
    }

    initializeQuestionsMenu();
}

function showQuizSummary() {
    const timeTaken = Date.now() - quizState.startTime;
    const attemptedQuestions = quizState.correctAnswersCount + quizState.wrongAnswerCount;
    const accuracy = attemptedQuestions > 0
        ? (quizState.correctAnswersCount / attemptedQuestions) * 100
        : 0;

    document.getElementById('accuracy-percentage').textContent = Math.round(accuracy);
    document.getElementById('correct-count').textContent = quizState.correctAnswersCount;
    document.getElementById('incorrect-count').textContent = quizState.wrongAnswerCount;
    document.getElementById('blank-count').textContent = quizState.blankAnswersCount;
    document.getElementById('time-taken').textContent = formatTime(timeTaken);

    const detailedList = document.getElementById('detailed-answers-list');
    detailedList.innerHTML = '';

    quizState.cards.forEach((card, index) => {
        const userAnswer = quizState.originalAnswers[index];
        const isCorrect = userAnswer.toLowerCase() === card.Term.toLowerCase();
        const isBlank = userAnswer === '';

        const listItem = document.createElement('div');
        listItem.className = 'answer-detail-item';

        let statusClass = isCorrect ? 'correct' : (isBlank ? 'blank' : 'incorrect');
        let statusIcon = isCorrect ? 'fi-br-check' : (isBlank ? 'fi-rr-minus' : 'fi-rr-cross-small');

        listItem.innerHTML = `
            <div class="question-number">${index + 1}</div>
            <div class="answer-content">
                <div class="question-text">${card.Meaning}</div>
                <div class="answer-text">
                    <span class="user-answer ${statusClass}">
                        ${isBlank ? 'Boş bırakıldı' : userAnswer}
                        <i class="fi ${statusIcon}"></i>
                    </span>
                    ${!isCorrect ? `<span class="correct-answer">Doğru cevap: ${card.Term}</span>` : ''}
                </div>
            </div>
        `;

        detailedList.appendChild(listItem);
    });

    updateProgressCircle(accuracy);

    document.getElementById('quiz-summary').style.display = 'block';
    quizState.isQuizCompleted = true;
    showCard(quizState.currentIndex);
}

async function submitQuizResults() {
    const quizData = {
        deckId: quizState.deckId,
        totalQuestions: quizState.cards.length,
        correctAnswers: quizState.correctAnswersCount,
        timeTaken: Date.now() - quizState.startTime,
        answerDetails: quizState.quizAnswers
    };

    try {
        const response = await fetch(quizState.quizResultsUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(quizData)
        });

        if (!response.ok) {
            new Error('Network response was not ok');
        }

        const result = await response.json();
        if (result.redirectUrl) {
            window.location.href = result.redirectUrl;
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to submit quiz results. Please try again.');
    }
}

function updateProgressCircle(percentage) {
    const circle = document.querySelector('.progress-circle .progress');
    const backgroundCircle = document.querySelector('.progress-circle .background');
    const radius = circle.getAttribute('r');
    const circumference = 2 * Math.PI * radius;

    const correctPortion = (percentage / 100) * circumference;
    const incorrectPortion = circumference - correctPortion;

    circle.style.strokeDasharray = `${correctPortion} ${circumference}`;
    circle.style.strokeDashoffset = '0';

    backgroundCircle.style.strokeDasharray = `${incorrectPortion} ${circumference}`;
    backgroundCircle.style.strokeDashoffset = `-${correctPortion}`;
}

function formatTime(milliseconds) {
    const totalSeconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

window.initializeQuiz = initializeQuiz;
window.toggleMenu = toggleMenu;
window.submitAnswer = submitAnswer;
window.showPreviousCard = showPreviousCard;
