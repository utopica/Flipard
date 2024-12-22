var cardNumber = 1;
let globalScanInput = null;

document.addEventListener('DOMContentLoaded', function() {
    cardNumber = document.querySelectorAll('.user-deck').length;

    globalScanInput = document.createElement('input');
    globalScanInput.type = 'file';
    globalScanInput.id = 'scan-input';
    globalScanInput.style.display = 'none';
    globalScanInput.accept = '.pdf,.jpg,.jpeg,.png,.bmp,.tiff';
    document.body.appendChild(globalScanInput);

    setupEventListeners();
    updateDeleteButtonListeners();
});

function setupEventListeners() {
    document.addEventListener('click', function(event) {
        const scanButton = event.target.closest('.card-scan-button');
        if (scanButton) {
            const targetType = scanButton.getAttribute('data-target');
            handleScanButtonClick(scanButton, targetType);
        }

        const imageButton = event.target.closest('.card-image-button');
        if (imageButton) {
            const fileInput = imageButton.closest('.card-edit-buttons').querySelector('.card-image-input');
            fileInput.click();
        }
    });

    document.addEventListener('change', function(event) {
        if (event.target.classList.contains('card-image-input')) {
            handleImageSelection(event.target);
        }
    });
}

function handleImageSelection(fileInput) {
    const card = fileInput.closest('.user-deck');
    const file = fileInput.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const imgPreview = card.querySelector('.image-preview');
            const imageIcon = card.querySelector('.fi-tr-graphic-style');
            const imageUrlInput = card.querySelector('.card-image-url');

            imgPreview.src = e.target.result;
            imgPreview.style.display = 'block';
            imageIcon.style.display = 'none';
            imageUrlInput.value = e.target.result;
        };
        reader.readAsDataURL(file);
    }
}

async function handleScanButtonClick(button, targetType) {
    globalScanInput.onchange = async () => {
        const file = globalScanInput.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch('/Ocr/UploadFileJson', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (result.text) {
                const extractedText = result.text.trim();
                const targetInput = targetType === 'term'
                    ? button.closest('.user-deck').querySelector('.user-deck-card-term-input')
                    : button.closest('.user-deck').querySelector('.user-deck-card-definition-input');

                targetInput.value = extractedText;
            } else {
                alert(result.error || 'An error occurred during text extraction.');
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Failed to extract text. Please try again.');
        }
    };

    globalScanInput.click();
}

function updateDeleteButtonListeners() {
    document.querySelectorAll('.card-delete-button').forEach(button => {
        button.removeEventListener('click', handleDeleteCard);
        button.addEventListener('click', handleDeleteCard);
    });
}

function handleDeleteCard(event) {
    const cardElement = event.target.closest('.user-deck');
    if (cardElement && confirm('Are you sure you want to delete this card?')) {
        cardElement.remove();
        updateCardNumbers();
    }
}

function updateCardNumbers() {
    document.querySelectorAll('.user-deck').forEach((card, index) => {
        card.querySelector('.card-number').textContent = index + 1;

        const termInput = card.querySelector('.user-deck-card-term-input');
        const meaningInput = card.querySelector('.user-deck-card-definition-input');
        const imageInput = card.querySelector('.card-image-input');
        const imageUrlInput = card.querySelector('.card-image-url');

        if (termInput) termInput.name = `TermMeanings[${index}].Term`;
        if (meaningInput) meaningInput.name = `TermMeanings[${index}].Meaning`;
        if (imageInput) imageInput.name = `TermMeanings[${index}].Image`;
        if (imageUrlInput) imageUrlInput.name = `TermMeanings[${index}].ImageUrl`;
    });

    cardNumber = document.querySelectorAll('.user-deck').length;
}

document.getElementById("add-card-button").addEventListener("click", function () {
    var cardsContainer = document.querySelector('.cards-container'); // Changed to use querySelector
    if (!cardsContainer) {
        console.error('Cards container not found');
        return;
    }

    var newCardHtml = `
        <div class="user-deck">
            <div class="user-deck-card-info">
               <div class="user-deck-card-term-buttons">
                        <span class="card-number">${cardNumber + 1}</span>
                        
                        <button type="button" class="card-scan-button" data-type="scan" data-target="term">
                            <span class="button-card-scan-icon">
                                <i class="fi fi-br-qr-scan"></i>
                            </span>
                        </button>
                    </div>
                <div class="card-edit-buttons">
                        <button type="button" class="card-delete-button">
                            <span class="button-card-delete-icon">
                                <i class="fa-solid fa-trash"></i>
                            </span>
                        </button>
                        <button type="button" class="card-image-button" data-type="image">
                            <span class="button-card-image-icon">
                                <i class="fa-solid fa-image"></i>
                            </span>
                        </button>
                        <button type="button" class="card-scan-button" data-type="scan" data-target="meaning">
                            <span class="button-card-scan-icon">
                                <i class="fi fi-br-qr-scan"></i>
                            </span>
                        </button>
                        <input type="file" class="card-image-input" name="TermMeanings[${cardNumber}].Image" style="display: none;">
                    </div>
            </div>
            <div class="user-deck-card">
                <div class="user-deck-card-term">
                    <textarea class="user-deck-card-term-input" name="TermMeanings[${cardNumber}].Term" placeholder="Terim"></textarea>
                </div>
                <div class="user-deck-card-definition">
                    <textarea class="user-deck-card-definition-input" name="TermMeanings[${cardNumber}].Meaning" placeholder="Tanım"></textarea>
                </div>
                <div class="user-deck-card-image">
                    <img class="image-preview" style="display:none;" alt="" src=""/>
                    <i class="fi fi-tr-graphic-style"></i>
                    <input type="hidden" name="TermMeanings[${cardNumber}].ImageUrl" class="card-image-url"/>
                </div>
            </div>
        </div>
    `;

    var tempContainer = document.createElement('div');
    tempContainer.innerHTML = newCardHtml;

    var newCard = tempContainer.firstElementChild;
    cardsContainer.insertBefore(newCard, cardsContainer.querySelector('.add-card-button-div'));

    cardNumber++;
    updateDeleteButtonListeners();
});