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
    isQuizCompleted: false,
    questionTypes: [],
    currentQuestionType: null
};

function initializeQuiz(quizCards, deck, resultsUrl) {
    const urlParams = new URLSearchParams(window.location.search);
    const settingsParam = urlParams.get('settings');

    let settings = {
        modes: {feedback: true},
        questionCount: quizCards.length,
        answerWith: 'term',
        questionTypes: {
            written: true,
            multipleChoice: true,
            trueFalse: true,
        }
    };

    if (settingsParam) {
        try {
            const parsedSettings = JSON.parse(settingsParam);
            settings = {...settings, ...parsedSettings};
        } catch (e) {
            console.error('Failed to parse quiz settings:', e);
        }
    }

    // Generate question types array based on settings
    quizState.questionTypes = generateQuestionTypeArray(settings.questionTypes, settings.questionCount);

    // Shuffle and slice cards
    quizState.cards = shuffleArray(quizCards).slice(0, settings.questionCount);
    quizState.deckId = deck;
    quizState.quizResultsUrl = resultsUrl;
    quizState.originalAnswers = new Array(settings.questionCount).fill('');
    quizState.startTime = Date.now();
    quizState.isQuizCompleted = false;
    quizState.feedbackMode = settings.modes.feedback;
    quizState.answerWith = settings.answerWith;

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

function generateQuestionTypeArray(types, count) {
    const enabledTypes = Object.entries(types)
        .filter(([_, enabled]) => enabled)
        .map(([type]) => type);

    if (enabledTypes.length === 0) {
        enabledTypes.push('written'); // Fallback to written if no types selected
    }

    const questionTypes = [];
    for (let i = 0; i < count; i++) {
        questionTypes.push(enabledTypes[i % enabledTypes.length]);
    }
    return shuffleArray(questionTypes);
}

function initializeQuestionsMenu() {
    const grid = document.getElementById('questionsGrid');
    if (!grid) return;

    grid.innerHTML = '';

    for (let i = 0; i < quizState.cards.length; i++) {
        const button = document.createElement('button');
        button.className = getQuestionButtonClass(i);
        button.textContent = (i + 1).toString();

        button.onclick = () => {
            showCard(i);
        }

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
        if (answer.isBlank) {
            baseClasses.push('blank');
        } else {
            baseClasses.push('answered');
            if (quizState.feedbackMode) {
                if (answer.isCorrect) {
                    baseClasses.push('correct');
                } else {
                    baseClasses.push('incorrect');
                }
            }
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
        quizState.currentQuestionType = quizState.questionTypes[index];

        initializeQuestionsMenu();

        const meaningContent = document.querySelector(".card-meaning-content");
        meaningContent.innerHTML = '';

        const textDiv = document.createElement("div");
        textDiv.id = "card-meaning-text";
        textDiv.className = "card-meaning-text";
        textDiv.textContent = quizState.cards[index].Meaning;
        meaningContent.appendChild(textDiv);

        if (quizState.cards[index].ImageUrl) {
            const imageDiv = document.createElement("div");
            imageDiv.id = "card-meaning-image";
            imageDiv.className = "card-meaning-image";

            const img = document.createElement("img");
            img.src = quizState.cards[index].ImageUrl;
            img.alt = "Card Image";

            imageDiv.appendChild(img);
            meaningContent.appendChild(imageDiv);

            textDiv.style.width = window.innerWidth <= 500 ? "220px" :
                window.innerWidth <= 768 ? "280px" : "350px";
        } else {
            textDiv.style.width = "100%";
        }

        // resetting containers
        const termContainer = document.getElementById("card-term-container");
        const feedbackContainer = document.getElementById("feedback-container");
        termContainer.style.display = 'block';
        feedbackContainer.style.display = 'none';

        switch (quizState.currentQuestionType) {
            case 'written':
                showWrittenQuestion(termContainer);
                break;
            case 'multipleChoice':
                showMultipleChoiceQuestion(termContainer);
                break;
            case 'trueFalse':
                showTrueFalseQuestion(termContainer);
                break;
        }

        const previousAnswer = quizState.answeredQuestions.find(
            q => q.questionIndex === quizState.currentIndex
        );

        if (previousAnswer?.feedbackShown) {
            if (quizState.currentQuestionType === 'written') {
                    showFeedback(previousAnswer.userAnswer, quizState.cards[index].Term, quizState.currentQuestionType);
            } else {
                // For multiple choice and true/false, update the UI to show correct/incorrect states
                const buttons = termContainer.querySelectorAll('button');
                buttons.forEach(button => {
                    if (quizState.currentQuestionType === 'multipleChoice') {
                        if (button.textContent === quizState.cards[index].Term) {
                            button.classList.add('correct');
                        } else if (button.textContent === previousAnswer.userAnswer) {
                            button.classList.add('incorrect');
                        }
                    } else if (quizState.currentQuestionType === 'trueFalse') {
                        const isShowingCorrect = getTrueFalseCorrectness(index);
                        const correctAnswer = isShowingCorrect ? 'True' : 'False';
                        if (button.textContent === correctAnswer) {
                            button.classList.add('correct');
                        } else if (button.textContent === previousAnswer.userAnswer) {
                            button.classList.add('incorrect');
                        }
                    }
                });

                // Show the correct term for true/false questions
                if (quizState.currentQuestionType === 'trueFalse') {
                    const termDisplay = termContainer.querySelector('.true-false-term');
                    if (termDisplay) {
                        termDisplay.textContent = quizState.cards[index].Term;
                    }
                }
            }
        }

        updateProgress(index);
    }
}

function updateProgress(index) {
    const progress = document.querySelector(".card-heading #progress");
    if (progress) {
        progress.textContent = `${index + 1}/${quizState.cards.length}`;
    }
}

function showWrittenQuestion(container) {
    container.innerHTML = '';
    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'card-term-text';
    input.id = 'term-input';
    input.placeholder = 'Enter your answer';

    const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === quizState.currentIndex);
    if (previousAnswer) {
        input.value = previousAnswer.userAnswer;
        if (quizState.feedbackMode && previousAnswer.feedbackShown) {
            input.disabled = true;
        }
    }

    input.disabled = quizState.isQuizCompleted;
    container.appendChild(input);

    if (!input.disabled) {
        input.focus();
    }
}

function showMultipleChoiceQuestion(container) {
    container.innerHTML = '';
    const questionIndex = quizState.currentIndex
    const options = getMultipleChoiceOptions(questionIndex);
    const buttonsContainer = document.createElement('div');
    buttonsContainer.className = 'multiple-choice-container';

    const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === questionIndex);
    const feedbackShown = previousAnswer?.feedbackShown;

    options.forEach((option) => {
        const button = document.createElement('button');
        button.className = 'multiple-choice-option';
        button.textContent = option;
        button.disabled = feedbackShown || quizState.isQuizCompleted;

        if (feedbackShown) {
            if (option === quizState.cards[questionIndex].Term) {
                button.classList.add('correct');
            } else if (option === previousAnswer.userAnswer && !previousAnswer.isBlank) {
                button.classList.add('incorrect');
            }
        }

        button.onclick = () => {
            handleMultipleChoiceAnswer(option);
        };

        buttonsContainer.appendChild(button);
    });

    container.appendChild(buttonsContainer);

    if (previousAnswer?.isBlank && feedbackShown) {
        showBlankFeedback(container, quizState.cards[questionIndex].Term);
    }
}

function showTrueFalseQuestion(container) {
    container.innerHTML = '';
    const questionIndex = quizState.currentIndex;
    const currentCard = quizState.cards[questionIndex];
    const isShowingCorrect = getTrueFalseCorrectness(questionIndex);
    const displayedTerm = isShowingCorrect ?
        currentCard.Term :
        getRandomTermExcept(currentCard.Term);

    const termDisplay = document.createElement('div');
    termDisplay.className = 'true-false-term';
    termDisplay.textContent = displayedTerm;

    const buttonsContainer = document.createElement('div');
    buttonsContainer.className = 'true-false-buttons';

    const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === questionIndex);
    const feedbackShown = previousAnswer?.feedbackShown;


    ['True', 'False'].forEach(option => {
        const button = document.createElement('button');
        button.className = 'true-false-option';
        button.textContent = option;
        button.disabled = feedbackShown || quizState.isQuizCompleted;

        if (feedbackShown) {
            const isCorrectAnswer = (option === 'True' && isShowingCorrect) ||
                (option === 'False' && !isShowingCorrect);

            if (isCorrectAnswer) {
                button.classList.add('correct');
            } else if (option === previousAnswer.userAnswer && !previousAnswer.isBlank) {
                button.classList.add('incorrect');
            }
        }

        button.onclick = () => handleTrueFalseAnswer(option, isShowingCorrect);
        buttonsContainer.appendChild(button);
    });

    container.appendChild(termDisplay);
    container.appendChild(buttonsContainer);

    if (previousAnswer?.isBlank && feedbackShown) {
        showBlankFeedback(container, isShowingCorrect ? 'True' : 'False');
    }
}

function showBlankFeedback(container, correctAnswer) {
    const blankFeedback = document.createElement('div');
    blankFeedback.className = 'blank-feedback';
    blankFeedback.innerHTML = `
        <div class="wrong-answer">
            Boş bırakıldı
            <i class="fi fi-rr-cross-small"></i>
        </div>
        <div class="correct-answer">
            ${correctAnswer}
            <i class="fi fi-br-check"></i>
        </div>
    `;
    container.appendChild(blankFeedback);
}

function submitAnswer() {

    const existingAnswer = quizState.answeredQuestions.find(
        q => q.questionIndex === quizState.currentIndex
    );

    // If in feedback mode and feedback was already shown, just advance to next question
    if (quizState.feedbackMode && existingAnswer?.feedbackShown) {
        advanceToNextQuestion();
        return;
    }

    if (quizState.currentQuestionType === 'written') {
        const termInput = document.getElementById('term-input');
        const userTerm = termInput.value.trim();
        const isBlank = userTerm === '';
        const isCorrect = !isBlank && (normalizeString(userTerm) === normalizeString(quizState.cards[quizState.currentIndex].Term));
        processAnswer(userTerm, isCorrect, isBlank);
    } else if (quizState.currentQuestionType === 'multipleChoice') {
        const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === quizState.currentIndex);
        const userAnswer = previousAnswer ? previousAnswer.userAnswer : '';
        const isBlank = userAnswer === '';
        const isCorrect = !isBlank && (userAnswer === quizState.cards[quizState.currentIndex].Term);
        processAnswer(userAnswer, isCorrect, isBlank);
    } else if (quizState.currentQuestionType === 'trueFalse') {
        const previousAnswer = quizState.answeredQuestions.find(q => q.questionIndex === quizState.currentIndex);
        const userAnswer = previousAnswer ? previousAnswer.userAnswer : '';
        const isBlank = userAnswer === '';
        const isShowingCorrect = getTrueFalseCorrectness(quizState.currentIndex);
        const isCorrect = !isBlank && ((userAnswer === 'True' && isShowingCorrect) || (userAnswer === 'False' && !isShowingCorrect));
        processAnswer(userAnswer, isCorrect, isBlank);
    }
}

function processAnswer(userAnswer, isCorrect, isBlank = false) {
    const currentCard = quizState.cards[quizState.currentIndex];
    const existingAnswerIndex = quizState.answeredQuestions.findIndex(
        q => q.questionIndex === quizState.currentIndex
    );

    // Create the answer object
    const answerObject = {
        questionIndex: quizState.currentIndex,
        userAnswer: userAnswer,
        isCorrect: isCorrect,
        isBlank: isBlank,
        vocabulary: currentCard.Id,
        questionType: quizState.currentQuestionType,
        feedbackShown: false
    };

    if (quizState.feedbackMode) {
        if (existingAnswerIndex === -1) {
            quizState.answeredQuestions.push(answerObject);
            updateStatistics(isBlank, isCorrect);
            quizState.originalAnswers[quizState.currentIndex] = userAnswer;
        }

        const currentAnswer = quizState.answeredQuestions.find(
            q => q.questionIndex === quizState.currentIndex
        );

        if (!currentAnswer.feedbackShown) {
            currentAnswer.feedbackShown = true;
            if (quizState.currentQuestionType === 'written') {
                // if (!currentAnswer.isCorrect) {
                //     showFeedback(userAnswer, currentCard.Term, quizState.currentQuestionType);
                // } else {
                //     advanceToNextQuestion(); // Immediately advance if written answer is correct
                // }

                showFeedback(userAnswer, currentCard.Term, quizState.currentQuestionType);
            } else {
                // For multiple choice and true/false, re-render the current card to show feedback
                showCard(quizState.currentIndex);
            }
        }
    } else {
        if (existingAnswerIndex !== -1) {
            const oldAnswer = quizState.answeredQuestions[existingAnswerIndex];
            revertStatistics(oldAnswer);
            quizState.answeredQuestions.splice(existingAnswerIndex, 1);
        }

        quizState.answeredQuestions.push(answerObject);
        updateStatistics(isBlank, isCorrect);
        quizState.originalAnswers[quizState.currentIndex] = userAnswer;
        advanceToNextQuestion();
    }

    initializeQuestionsMenu();
}

function updateStatistics(isBlank, isCorrect) {
    if (isBlank) quizState.blankAnswersCount++;
    else if (isCorrect) quizState.correctAnswersCount++;
    else quizState.wrongAnswerCount++;
}

function revertStatistics(answer) {
    if (answer.isBlank) quizState.blankAnswersCount--;
    else if (answer.isCorrect) quizState.correctAnswersCount--;
    else quizState.wrongAnswerCount--;
}

function showFeedback(userAnswer, correctAnswer, questionType) {
    const termInputContainer = document.getElementById("card-term-container");
    const feedbackContainer = document.getElementById("feedback-container");

    if (questionType === 'written') {
        termInputContainer.style.display = 'none';
        feedbackContainer.style.display = 'block';

        // Keep the input visible but disabled during feedback
        const termInput = document.getElementById('term-input');
        if (termInput) {
            termInput.disabled = true;
            termInput.value = userAnswer;
        }

        const userAnswerDisplay = document.querySelector(".user-answer-display");
        const answerFeedback = document.getElementById("answer-feedback");

        const isCorrect = normalizeString(userAnswer) === normalizeString(correctAnswer);

        userAnswerDisplay.innerHTML = `
            <div class="${isCorrect ? 'correct-answer' : 'wrong-answer'}">
                ${userAnswer === '' ? 'Boş bırakıldı' : userAnswer}
                <i class="fi ${isCorrect ? 'fi-br-check' : 'fi-rr-cross-small'}"></i>
            </div>`;

        // Only show the correct answer feedback if the user's answer was wrong
        if (!isCorrect) {
            answerFeedback.innerHTML = `
                <div class="correct-answer">
                    ${correctAnswer}
                    <i class="fi fi-br-check"></i>
                </div>`;
        }
        else {
            answerFeedback.innerHTML = ''; // Clear the feedback container if answer was correct
        }
    } else {
        // For multiple choice and true/false, feedback is handled through button styling
        feedbackContainer.style.display = 'none';
    }

    quizState.showAnswer = true;
}

function advanceToNextQuestion() {
    if (quizState.currentIndex < quizState.cards.length - 1) {
        showCard(quizState.currentIndex + 1);
    } else {
        showQuizSummary();
    }
}

function showPreviousCard() {
    if (quizState.currentIndex > 0) {
        showCard(quizState.currentIndex - 1);
    }
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

    quizState.answeredQuestions.sort((a, b) => a.questionIndex - b.questionIndex)
        .forEach((answer, index) => {
            const card = quizState.cards[answer.questionIndex];
            const listItem = document.createElement('div');
            listItem.className = 'answer-detail-item';

            let statusClass = answer.isCorrect ? 'correct' : (answer.isBlank ? 'blank' : 'incorrect');
            let statusIcon = answer.isCorrect ? 'fi-br-check' : (answer.isBlank ? 'fi-rr-minus' : 'fi-rr-cross-small');

            let questionTypeText = '';
            switch (answer.questionType) {
                case 'written':
                    questionTypeText = 'Written';
                    break;
                case 'multipleChoice':
                    questionTypeText = 'Multiple Choice';
                    break;
                case 'trueFalse':
                    questionTypeText = 'True/False';
                    break;
            }

            listItem.innerHTML = `
                <div class="question-number">${answer.questionIndex + 1}</div>
                <div class="answer-content">
                    <div class="question-text">
                        ${card.Meaning}
                    </div>
                    <div class="answer-text">
                        <span class="user-answer ${statusClass}">
                            ${answer.isBlank ? 'Boş bırakıldı' : answer.userAnswer}
                            <i class="fi ${statusIcon}"></i>
                        </span>
                        ${!answer.isCorrect ? `<span class="correct-answer">Doğru cevap: ${card.Term}</span>` : ''}
                    </div>
                </div>
            `;

            detailedList.appendChild(listItem);
        });

    updateProgressCircle(accuracy);
    submitQuizResults();

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
        answerDetails: quizState.answeredQuestions.map(answer => ({
            vocabularyId: answer.vocabulary,
            userAnswer: answer.userAnswer,
            isCorrect: answer.isCorrect,
            questionType: answer.questionType
        }))
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
            throw new Error('Network response was not ok');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to submit quiz results. Please try again.');
    }
}

function getMultipleChoiceOptions(questionIndex) {
    const seed = questionIndex;
    return generateMultipleChoiceOptionsWithSeed(seed);
}

function generateMultipleChoiceOptionsWithSeed(seed) {
    const currentCard = quizState.cards[quizState.currentIndex];
    const correctAnswer = currentCard.Term;

    // Use seed to consistently select the same wrong options
    const otherOptions = quizState.cards
        .filter(card => card.Term !== correctAnswer)
        .map(card => card.Term);

    // Deterministic shuffle based on seed
    const shuffledOptions = deterministicShuffle(otherOptions, seed);
    const wrongOptions = shuffledOptions.slice(0, 3);

    return deterministicShuffle([correctAnswer, ...wrongOptions], seed);
}

function getTrueFalseCorrectness(questionIndex) {
    // Use a deterministic approach based on question index
    return (questionIndex % 2) === 0;
}

function deterministicShuffle(array, seed) {
    const shuffled = [...array];
    for (let i = shuffled.length - 1; i > 0; i--) {
        const j = (seed * (i + 1)) % (i + 1);
        [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }
    return shuffled;
}

function getRandomTermExcept(excludeTerm) {
    const otherTerms = quizState.cards
        .filter(card => card.Term !== excludeTerm)
        .map(card => card.Term);
    return otherTerms[Math.floor(Math.random() * otherTerms.length)];
}

function handleMultipleChoiceAnswer(selectedAnswer) {
    const isCorrect = selectedAnswer === quizState.cards[quizState.currentIndex].Term;
    processAnswer(selectedAnswer, isCorrect);
}

function handleTrueFalseAnswer(answer, isCorrectMatch) {
    const isCorrect = (answer === 'True' && isCorrectMatch) || (answer === 'False' && !isCorrectMatch);
    processAnswer(answer, isCorrect);
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

// Event listener setup
document.addEventListener("DOMContentLoaded", function () {

    const initialCount = document.getElementById("questionCount")?.getAttribute("value");
    maxQuestionCount = parseInt(initialCount) || 1;

    const questionCountInput = document.getElementById("questionCount");
    if (questionCountInput) {
        questionCountInput.value = maxQuestionCount;
        questionCountInput.addEventListener("input", validateQuestionCount);
    }

    const termInput = document.getElementById('term-input');
    if (termInput) {
        termInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                submitAnswer();
            }
        });
    }
});

let maxQuestionCount = 1; // Global variable to store max questions

function validateQuestionCount() {
    const questionCountInput = document.getElementById("questionCount");
    const value = parseInt(questionCountInput.value) || 1; // Use || 1 to handle NaN cases

    // Ensure value is between 1 and maxQuestionCount
    if (value < 1) {
        questionCountInput.value = 1;
    } else if (value > maxQuestionCount) {
        questionCountInput.value = maxQuestionCount;
    }
}

function normalizeString(str) {
    return str
        .toLowerCase()
        // .replace(/i̇/g, 'i') // Handle Turkish dotted i
        // .replace(/ı/g, 'i') // Handle Turkish dotless i
        // .replace(/İ/g, 'i') // Handle Turkish capital dotted I
        // .replace(/I/g, 'i') // Handle Turkish capital dotless I
        .normalize('NFKD') // Decompose combined characters
        .replace(/[\u0300-\u036f]/g, '') // Remove diacritics
        .replace(/\s+/g, ' ') // Normalize spaces
        .trim();
}
// Export functions for global access
window.initializeQuiz = initializeQuiz;
window.toggleMenu = toggleMenu;
window.submitAnswer = submitAnswer;
window.showPreviousCard = showPreviousCard;