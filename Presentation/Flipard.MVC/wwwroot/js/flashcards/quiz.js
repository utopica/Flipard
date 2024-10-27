const quizState = {
    cards: [],
    currentIndex: 0,
    showAnswer: false,
    startTime: Date.now(),
    quizAnswers: [],
    correctAnswersCount: 0,
    originalAnswers: [],
    deckId: '',
    quizResultsUrl: '',
    answeredQuestions: [],
    isMenuCollapsed: false
};

function initializeQuiz(quizCards, deck, resultsUrl) {
    quizState.cards = quizCards;
    quizState.deckId = deck;
    quizState.quizResultsUrl = resultsUrl;
    quizState.originalAnswers = new Array(quizCards.length).fill('');
    quizState.startTime = Date.now();
    showCard(quizState.currentIndex);
    initializeQuestionsMenu();
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
        button.disabled = !isAnswered && i !== quizState.currentIndex;

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

        initializeQuestionsMenu();

        const meaningText = document.getElementById("card-meaning-text");
        if (meaningText) {
            meaningText.innerText = quizState.cards[index].Meaning;
        }

        const termInput = document.getElementById("term-input");
        if (termInput) {
            termInput.value = "";
        }

        const progress = document.querySelector(".card-heading #progress");
        if (progress) {
            progress.innerText = `${index + 1}/${quizState.cards.length}`;
        }

        const answerFeedback = document.getElementById("answer-feedback");
        if (answerFeedback) {
            answerFeedback.innerHTML = "";
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

        quizState.showAnswer = false;
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
    const userTerm = document.getElementById("term-input").value.trim();
    const correctTerm = quizState.cards[quizState.currentIndex].Term;
    const answerFeedback = document.getElementById("answer-feedback");

    if (!quizState.originalAnswers[quizState.currentIndex]) {
        quizState.originalAnswers[quizState.currentIndex] = userTerm;

        quizState.answeredQuestions.push({
            questionIndex: quizState.currentIndex,
            userAnswer: userTerm,
            isCorrect: userTerm.toLowerCase() === correctTerm.toLowerCase()
        });
    }

    const isCorrect = userTerm.toLowerCase() === correctTerm.toLowerCase();

    if (!quizState.showAnswer && !isCorrect) {
        answerFeedback.innerHTML = `
            <div class="wrong-answer">${userTerm} <i class="fi fi-rr-cross-small"></i></div>
            <div class="correct-answer">${correctTerm} <i class="fi fi-br-check"></i></div>`;
        document.getElementById("term-input").value = correctTerm;
        quizState.showAnswer = true;
        initializeQuestionsMenu();
        return;
    }

    if (!quizState.quizAnswers.some(answer => answer.vocabularyId === quizState.cards[quizState.currentIndex].Id)) {
        quizState.quizAnswers.push({
            vocabularyId: quizState.cards[quizState.currentIndex].Id,
            userAnswer: quizState.originalAnswers[quizState.currentIndex],
            isCorrect: quizState.originalAnswers[quizState.currentIndex].toLowerCase() === correctTerm.toLowerCase()
        });

        if (quizState.originalAnswers[quizState.currentIndex].toLowerCase() === correctTerm.toLowerCase()) {
            quizState.correctAnswersCount++;
        }
    }

    if (quizState.currentIndex < quizState.cards.length - 1) {
        showCard(quizState.currentIndex + 1);
    } else {
        showQuizSummary();
    }

    initializeQuestionsMenu();
}

function showQuizSummary() {
    const timeTaken = Date.now() - quizState.startTime;
    const accuracy = (quizState.correctAnswersCount / quizState.cards.length) * 100;
    const incorrectCount = quizState.cards.length - quizState.correctAnswersCount;

    document.getElementById('accuracy-percentage').textContent = Math.round(accuracy);
    document.getElementById('correct-count').textContent = quizState.correctAnswersCount;
    document.getElementById('incorrect-count').textContent = incorrectCount;
    document.getElementById('time-taken').textContent = formatTime(timeTaken);

    updateProgressCircle(accuracy);

    document.getElementById('quiz-summary').style.display = 'block';
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

