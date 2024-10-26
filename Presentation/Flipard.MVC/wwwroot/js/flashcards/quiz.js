let cards = [];
let currentIndex = 0;
let showAnswer = false;
let startTime = Date.now();
let quizAnswers = [];
let correctAnswersCount = 0;
let originalAnswers = [];
let deckId = '';
let quizResultsUrl = '';

function initializeQuiz(quizCards, deck, resultsUrl) {
    cards = quizCards;
    deckId = deck;
    quizResultsUrl = resultsUrl;
    originalAnswers = new Array(cards.length).fill('');
    showCard(currentIndex);
    startTime = Date.now();
}
function showCard(index) {
    if (index >= 0 && index < cards.length) {
        currentIndex = index;
        document.getElementById("card-meaning-text").innerText = cards[currentIndex].Meaning;
        document.getElementById("term-input").value = "";
        document.querySelector(".card-heading #progress").innerText = (currentIndex + 1) + "/" + cards.length;
        document.getElementById("answer-feedback").innerHTML = "";

        var imageElement = document.getElementById("meaning-image");
        
        if (cards[currentIndex].ImageUrl) {
            if (!imageElement) {
                var img = document.createElement("img");
                img.id = "meaning-image";
                img.src = cards[currentIndex].ImageUrl;
                img.alt = "Image";
                document.getElementById("card-meaning-image").appendChild(img);
            } else {
                imageElement.src = cards[currentIndex].ImageUrl;
                imageElement.style.display = "block";
            }
        } else {
            if (imageElement) {
                imageElement.style.display = "none";
            }
        }
        showAnswer = false;
    }
}

document.addEventListener("DOMContentLoaded", function () {
    showCard(currentIndex);
    startTime = Date.now();
});

function showPreviousCard() {
    if (currentIndex > 0) {
        showCard(currentIndex - 1);
    }
}

function submitAnswer() {
    var userTerm = document.getElementById("term-input").value.trim();
    var correctTerm = cards[currentIndex].Term;
    var answerFeedback = document.getElementById("answer-feedback");

    // if (userTerm === "") {
    //     alert("Please enter a term.");
    //     return;
    // }

    if (!originalAnswers[currentIndex]) {
        originalAnswers[currentIndex] = userTerm;
    }

    var isCorrect = userTerm.toLowerCase() === correctTerm.toLowerCase();

    if (!showAnswer && !isCorrect) {
        answerFeedback.innerHTML = `
                    <div class="wrong-answer">${userTerm} <i class="fi fi-rr-cross-small"></i></div>
                    <div class="correct-answer">${correctTerm} <i class="fi fi-br-check"></i></div>`;
        document.getElementById("term-input").value = correctTerm;
        showAnswer = true;
        return;
    }

    if (!quizAnswers.some(answer => answer.vocabularyId === cards[currentIndex].Id)) {
        quizAnswers.push({
            vocabularyId: cards[currentIndex].Id,
            userAnswer: originalAnswers[currentIndex], // Use the stored original answer
            isCorrect: originalAnswers[currentIndex].toLowerCase() === correctTerm.toLowerCase()
        });

        if (originalAnswers[currentIndex].toLowerCase() === correctTerm.toLowerCase()) {
            correctAnswersCount++;
        }
    }

    if (currentIndex < cards.length - 1) {
        showCard(currentIndex + 1);
    } else {
        showQuizSummary();
    }
}

function showQuizSummary() {
    const timeTaken = Date.now() - startTime;
    const accuracy = (correctAnswersCount / cards.length) * 100;
    const incorrectCount = cards.length - correctAnswersCount;

    document.getElementById('accuracy-percentage').textContent = Math.round(accuracy);
    document.getElementById('correct-count').textContent = correctAnswersCount;
    document.getElementById('incorrect-count').textContent = incorrectCount;
    document.getElementById('time-taken').textContent = formatTime(timeTaken);

    updateProgressCircle(accuracy);

    document.getElementById('quiz-summary').style.display = 'block';
}

async function submitQuizResults() {
    const quizData = {
        deckId: deckId,
        totalQuestions: cards.length,
        correctAnswers: correctAnswersCount,
        timeTaken: Date.now() - startTime,
        answerDetails: quizAnswers
    };

    try {
        const response = await fetch(quizResultsUrl, {
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
