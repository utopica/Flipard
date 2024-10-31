let currentIndex = 0;
let cards = []; // Will be populated from the server
let showingTerm = true;

function showCard(index) {
    if (index >= 0 && index < cards.length) {
        currentIndex = index;
        document.querySelector("#current-card .term").innerText = cards[currentIndex].Term;
        document.querySelector("#current-card .meaning").innerText = cards[currentIndex].Meaning;
        showTerm();
    }
}

function showTerm() {
    document.querySelector("#current-card").classList.remove("flipped");
    showingTerm = true;
}

function showMeaning() {
    document.querySelector("#current-card").classList.add("flipped");
    showingTerm = false;
}

function flipCard() {
    if (showingTerm) {
        showMeaning();
    } else {
        showTerm();
    }
}

function showPreviousCard() {
    if (currentIndex > 0) {
        showCard(currentIndex - 1);
    }
}

function showNextCard() {
    if (currentIndex < cards.length - 1) {
        showCard(currentIndex + 1);
    }
}

function deleteCard(button) {
    const cardId = button.getAttribute("data-card-id");

    fetch(`/Flashcards/DeleteCard/${cardId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (response.ok) {
                button.closest('.card-edit-bar').nextElementSibling.remove();
                button.closest('.card-edit-bar').remove();
                updateCardNumbers();
            } else {
                alert('Failed to delete card.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while deleting the card.');
        });
}

function updateCardNumbers() {
    const cardNumbers = document.querySelectorAll('.card-number');
    cardNumbers.forEach((number, index) => {
        number.textContent = index + 1;
    });
}

function deleteDeck(deckId) {
    fetch(`/Flashcards/DeleteDeck/${deckId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (response.ok) {
                window.location.href = '/Home/Index';
            } else {
                alert('Failed to delete deck.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while deleting the deck.');
        });
}

function editCard(cardId) {
    document.querySelector(`#card-term-display-${cardId}`).style.display = 'none';
    document.querySelector(`#card-meaning-display-${cardId}`).style.display = 'none';
    document.querySelector(`#card-term-${cardId}`).style.display = 'block';
    document.querySelector(`#card-meaning-${cardId}`).style.display = 'block';
    document.querySelector(`#card-term-${cardId}`).focus();
    document.querySelector(`.card-edit-bar .card-edit-button[onclick="editCard('${cardId}')"]`).style.display = 'none';
    document.querySelector(`.card-edit-bar .card-save-button[onclick="saveCard('${cardId}')"]`).style.display = 'inline-block';
}

function saveCard(cardId) {
    const term = document.querySelector(`#card-term-${cardId}`).value;
    const meaning = document.querySelector(`#card-meaning-${cardId}`).value;

    const updatedCard = {
        Id: cardId,
        Term: term,
        Meaning: meaning
    };

    fetch(`/Flashcards/UpdateCard`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(updatedCard)
    })
        .then(response => {
            if (response.ok) {
                document.querySelector(`#card-term-display-${cardId}`).innerText = term;
                document.querySelector(`#card-meaning-display-${cardId}`).innerText = meaning;
                document.querySelector(`#card-term-display-${cardId}`).style.display = 'block';
                document.querySelector(`#card-meaning-display-${cardId}`).style.display = 'block';
                document.querySelector(`#card-term-${cardId}`).style.display = 'none';
                document.querySelector(`#card-meaning-${cardId}`).style.display = 'none';
                document.querySelector(`.card-edit-bar .card-edit-button[onclick="editCard('${cardId}')"]`).style.display = 'inline-block';
                document.querySelector(`.card-edit-bar .card-save-button[onclick="saveCard('${cardId}')"]`).style.display = 'none';
            } else {
                alert('Failed to save card.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while saving the card.');
        });
}

function redirectToQuiz(deckId) {
    window.location.href = '/Flashcards/CreateQuiz/' + deckId;
}

function showQuizSettings() {
    document.getElementById('quizSettingsModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeQuizSettings() {
    document.getElementById('quizSettingsModal').style.display = 'none';
    document.body.style.overflow = '';
}

function saveQuizSettings() {
    const settings = {
        modes: {
            feedback: document.getElementById('feedbackMode').checked
        },
        questionCount: parseInt(document.getElementById('questionCount').value),
        answerWith: document.getElementById('answerWithTerm').checked ? 'term' : 'definition',
        questionTypes: {
            written: document.getElementById('writtenType').checked,
            multipleChoice: document.getElementById('multipleChoiceType').checked,
            trueFalse: document.getElementById('trueFalseType').checked,
        }
    };

    localStorage.setItem('quizSettings', JSON.stringify(settings));
    closeQuizSettings();
}

function setQuizOptions(deckId) {
    showQuizSettings();
}

function initializeEventListeners() {
    document.getElementById('answerWithTerm').addEventListener('change', function(e) {
        if (e.target.checked) {
            document.getElementById('answerWithDefinition').checked = false;
        }
    });

    document.getElementById('answerWithDefinition').addEventListener('change', function(e) {
        if (e.target.checked) {
            document.getElementById('answerWithTerm').checked = false;
        }
    });

    window.onclick = function(event) {
        if (event.target === document.getElementById('quizSettingsModal')) {
            closeQuizSettings();
        }
    };
}

function loadSavedSettings() {
    const savedSettings = localStorage.getItem('quizSettings');
    if (savedSettings) {
        const settings = JSON.parse(savedSettings);

        document.getElementById('feedbackMode').checked = settings.modes.feedback;
        document.getElementById('questionCount').value = settings.questionCount;
        document.getElementById('answerWithTerm').checked = settings.answerWith === 'term';
        document.getElementById('answerWithDefinition').checked = settings.answerWith === 'definition';
        document.getElementById('writtenType').checked = settings.questionTypes.written;
        document.getElementById('multipleChoiceType').checked = settings.questionTypes.multipleChoice;
        document.getElementById('trueFalseType').checked = settings.questionTypes.trueFalse;
    }
}

function initialize(serverCards) {
    cards = serverCards;
    showCard(currentIndex);

    const questionCountInput = document.getElementById('questionCount');
    if (questionCountInput) {
        questionCountInput.max = cards.length;
    }

    initializeEventListeners();
    loadSavedSettings();
}