let currentIndex = 0;
let cards = []; // Will be populated from the server
let showingTerm = true;
let deckIdToDelete = null;
let deckIdToEdit = null;

function initialize(serverCards) {
    cards = serverCards;
    showCard(currentIndex);
    renderCards(cards);
    initializeEventListeners();
    loadSavedSettings();
}

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

function renderCards(cards) {
    const cardList = document.getElementById("card-list");
    cardList.innerHTML = ""; // Clear any existing content

    cards.forEach((card, index) => {
        const cardElement = document.createElement("div");
        cardElement.classList.add("card");

        cardElement.innerHTML = `
            <div class="card-edit-bar">
                <span class="card-number">${index + 1}</span>
                <div class="card-edit-buttons">
                    ${!card.isReadOnly ? `
                        <button type="button" class="card-delete-button" data-card-id="${card.Id}" onclick="deleteCard(this)">
                            <span class="button-card-delete-icon"><i class="fa-solid fa-trash"></i></span>
                        </button>
                        <button type="button" class="card-edit-button" onclick="editCard('${card.Id}')">
                            <span class="button-card-edit-icon"><i class="fa-solid fa-pen"></i></span>
                        </button>
                        <button type="button" class="card-save-button" onclick="saveCard('${card.Id}')" style="display: none;">
                            <span class="button-card-save-icon"><i class="fa-solid fa-floppy-disk"></i></span>
                        </button>
                    ` : ""}
                </div>
            </div>
            <div class="card-content">
                <div class="card-term-meaning">
                    <div class="card-term">
                        <span id="card-term-display-${card.Id}">${card.Term}</span>
                        ${!card.isReadOnly ? `<textarea class="card-term" id="card-term-${card.Id}" style="display: none;">${card.Term}</textarea>` : ""}
                    </div>
                    <div class="card-content-divider"></div>
                    <div class="card-meaning">
                        <span class="card-meaning" id="card-meaning-display-${card.Id}">${card.Meaning}</span>
                        ${!card.isReadOnly ? `<textarea class="card-term" id="card-meaning-${card.Id}" style="display: none;">${card.Meaning}</textarea>` : ""}
                    </div>
                </div>
                <div class="card-image">
                    ${card.ImageUrl
            ? `<img class="image-preview" src="${card.ImageUrl}" />`
            : `<i class="fi fi-tr-graphic-style"></i>`
        }
                </div>
            </div>
        `;
        cardList.appendChild(cardElement);
    });
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
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!token) {
        console.error('Anti-forgery token not found');
        alert('Güvenlik doğrulaması başarısız oldu. Sayfayı yenileyip tekrar deneyin.');
        return;
    }
    
    fetch(`/Flashcards/DeleteDeck/${deckId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
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
    document.getElementById('answerWithTerm').addEventListener('change', function (e) {
        if (e.target.checked) {
            document.getElementById('answerWithDefinition').checked = false;
        }
    });

    document.getElementById('answerWithDefinition').addEventListener('change', function (e) {
        if (e.target.checked) {
            document.getElementById('answerWithTerm').checked = false;
        }
    });

    window.onclick = function (event) {
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

function redirectToQuiz(deckId) {
    const savedSettings = localStorage.getItem('quizSettings');
    if (savedSettings) {
        const settings = JSON.parse(savedSettings);
        const params = new URLSearchParams({
            settings: JSON.stringify(settings)
        });
        window.location.href = `/Flashcards/CreateQuiz/${deckId}?${params.toString()}`;
    } else {
        window.location.href = '/Flashcards/CreateQuiz/' + deckId;
    }
}

function showDeleteConfirmation(deckId) {
    deckIdToDelete = deckId;
    const modal = document.getElementById('deleteConfirmationModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
    } else {
        console.error('Delete confirmation modal not found');
        if (confirm('Çalışma setini silmek istediğinizden emin misiniz?')) {
            deleteDeck(deckId);
        }
    }
}

function closeDeleteConfirmation() {
    const modal = document.getElementById('deleteConfirmationModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }
    deckIdToDelete = null;
}

function confirmDeleteDeck() {
    if (deckIdToDelete) {
        deleteDeck(deckIdToDelete);
    }
    closeDeleteConfirmation();
}

function redirectToEdit(deckId) {
    if (!deckId || deckId.trim() === "") {
        alert("Deck ID is required.");
        return;
    }
    // Redirect to the EditQuiz page with the deckId as a query parameter
    window.location.href = `/Flashcards/EditSet/${deckId}`;
}

window.onclick = function(event) {
    const modal = document.getElementById('deleteConfirmationModal');
    if (event.target === modal) {
        closeDeleteConfirmation();
    }
}

// Handle Escape key
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeDeleteConfirmation();
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const questionCountInput = document.getElementById("questionCount");
    if (!questionCountInput.value) {
        questionCountInput.value = serverCards.length; // Default to serverCards length
    }
});

function validateQuestionCount() {
    const questionCountInput = document.getElementById("questionCount");
    const maxCount = cards.length;
    const value = parseInt(questionCountInput.value);

    if (value < 1) {
        questionCountInput.value = 1;
    } else if (value > maxCount) {
        questionCountInput.value = maxCount;
    }
}

