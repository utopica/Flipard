var cardNumber = 1;
let globalScanInput = null;

document.getElementById("add-card-button").addEventListener("click", function () {
    var newCardHtml = `
        <div class="user-deck">
            <div class="user-deck-card-info">
               <div class="user-deck-card-term-buttons">
                        <span class="card-number">1</span>
                        
                        <button type="button" class="card-scan-button" data-type="scan" data-target="term" onclick="handleScanButtonClick(this)">
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
                        <button type="button" class="card-scan-button" data-type="scan" data-target="meaning" onclick="handleScanButtonClick(this)">
                            <span class="button-card-scan-icon">
                                <i class="fi fi-br-qr-scan"></i>
                            </span>
                        </button>
                        <input type="file" class="card-image-input" name="TermMeanings[0].Image" style="display: none;">
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

    var newCardContainer = document.createElement("div");
    newCardContainer.innerHTML = newCardHtml;
    document.getElementById("cards-container").appendChild(newCardContainer);
    cardNumber++;
    updateDeleteButtonListeners();
});

document.addEventListener('DOMContentLoaded', function() {
    // Create a single hidden input for scanning
    globalScanInput = document.createElement('input');
    globalScanInput.type = 'file';
    globalScanInput.id = 'scan-input';
    globalScanInput.style.display = 'none';
    globalScanInput.accept = '.pdf,.jpg,.jpeg,.png,.bmp,.tiff';
    document.body.appendChild(globalScanInput);

    document.addEventListener('click', function(event) {
        const scanButton = event.target.closest('.card-scan-button');
        if (scanButton) {
            const targetType = scanButton.getAttribute('data-target');
            handleScanButtonClick(scanButton, targetType);
        }

        // Image button handler
        const imageButton = event.target.closest('.card-image-button');
        if (imageButton) {
            const fileInput = imageButton.closest('.card-edit-buttons').querySelector('.card-image-input');
            fileInput.click();
        }
    });

    document.addEventListener('change', function(event) {
        if (event.target.classList.contains('card-image-input')) {
            const fileInput = event.target;
            const card = fileInput.closest('.user-deck');
            const file = fileInput.files[0];

            if (file) {
                const reader = new FileReader();

                reader.onload = function (e) {
                    const imgPreview = card.querySelector(".image-preview");
                    imgPreview.src = e.target.result;
                    imgPreview.style.display = 'block';

                    const imageIcon = card.querySelector(".fi-tr-graphic-style");
                    if (imageIcon) {
                        imageIcon.style.display = 'none';
                    }

                    const imageUrlInput = card.querySelector(".card-image-url");
                    imageUrlInput.value = e.target.result;
                };

                reader.readAsDataURL(file);
            }
        }
    });
});

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
                const extractedText = result.text.trim(); // Trim whitespace

                if (targetType === 'term') {
                    button.closest('.user-deck').querySelector('.user-deck-card-term-input').value = extractedText;
                } else if (targetType === 'meaning') {
                    button.closest('.user-deck').querySelector('.user-deck-card-definition-input').value = extractedText;
                }
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
    var cardElement = event.target.closest('.user-deck');
    cardElement.remove();
    updateCardNumbers();
}

function updateCardNumbers() {
    document.querySelectorAll('.user-deck').forEach((card, index) => {
        card.querySelector('.card-number').textContent = index + 1;
        card.querySelector('.user-deck-card-term-input').name = `TermMeanings[${index}].Term`;
        card.querySelector('.user-deck-card-definition-input').name = `TermMeanings[${index}].Meaning`;
        card.querySelector('.card-image-input').name = `TermMeanings[${index}].Image`;
        card.querySelector('.card-image-url').name = `TermMeanings[${index}].ImageUrl`;
    });
    cardNumber = document.querySelectorAll('.user-deck').length;
}

updateDeleteButtonListeners();