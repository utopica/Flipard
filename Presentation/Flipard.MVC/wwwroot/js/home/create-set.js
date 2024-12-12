var cardNumber = 1;

document.getElementById("add-card-button").addEventListener("click", function () {
    var newCardHtml = `
                <div class="user-deck">
                    <div class="user-deck-card-info">
                        <!--card number, delete card button, add image button-->
                        <span class="card-number">${cardNumber + 1}</span>
                        <div>
                            <button type="button" class="card-delete-button">
                                <span class="button-card-delete-icon">
                                    <i class="fa-solid fa-trash"></i>
                                </span>
                            </button>
                            <button type="button" class="card-image-button">
                                <span class="button-card-image-icon">
                                    <i class="fa-solid fa-image"></i>
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
                            <img class="image-preview" style="display:none;" />
                            <i class="fi fi-tr-graphic-style"></i>
                            <input type="hidden" name="TermMeanings[${cardNumber}].ImageUrl" class="card-image-url" />
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

document.addEventListener("click", function (event) {
    if (event.target.closest(".card-image-button")) {
        var imageButton = event.target.closest(".card-image-button");
        var fileInput = imageButton.nextElementSibling;
        fileInput.click();
    }
});

document.addEventListener("change", function (event) {
    if (event.target.classList.contains("card-image-input")) {
        var fileInput = event.target;
        var card = fileInput.closest(".user-deck");
        var reader = new FileReader();
        reader.onload = function (e) {
            var imgPreview = card.querySelector(".image-preview");
            if (!imgPreview) {
                imgPreview = document.createElement("img");
                imgPreview.classList.add("image-preview");
                card.querySelector(".user-deck-card-image").appendChild(imgPreview);
            }
            imgPreview.src = e.target.result;
            imgPreview.style.display = 'block';
            card.querySelector(".fi-tr-graphic-style").style.display = 'none'; // Hide the icon

            var imageUrlInput = card.querySelector(".card-image-url");
            imageUrlInput.value = e.target.result;
        };
        reader.readAsDataURL(fileInput.files[0]);
    }
});

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

function navigateToUploadFile() {
    window.location.href = '/Ocr/UploadFile';
}