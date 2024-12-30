document.addEventListener('DOMContentLoaded', function() {
    const photoWrapper = document.querySelector('.profile-photo-wrapper');
    const photoInput = document.querySelector('.photo-input');
    const profilePhoto = document.querySelector('.profile-photo');
    let originalPhotoSrc = profilePhoto.src; // Store the original photo URL

    photoWrapper.addEventListener('click', function() {
        photoInput.click();
    });

    photoInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            // Validate file size (e.g., max 5MB)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                photoInput.value = ''; // Clear the input
                profilePhoto.src = originalPhotoSrc; // Restore original photo
                return;
            }

            // Validate file type
            if (!file.type.match('image.*')) {
                alert('Please select an image file');
                photoInput.value = ''; // Clear the input
                profilePhoto.src = originalPhotoSrc; // Restore original photo
                return;
            }

            const reader = new FileReader();
            reader.onload = function(event) {
                profilePhoto.src = event.target.result;
                profilePhoto.style.display = 'block';
            };
            reader.readAsDataURL(file);
        } else {
            // If no file is selected (user cancelled), restore original photo
            profilePhoto.src = originalPhotoSrc;
        }
    });

    // Handle form submission - prevent page refresh if update fails
    const form = document.querySelector('form');
    form.addEventListener('submit', function(e) {
        // Store the current preview in case form submission fails
        const currentPreview = profilePhoto.src;

        // Add event listener for when the page starts to unload
        window.addEventListener('unload', function() {
            // If page is refreshing/redirecting, keep the preview
            sessionStorage.setItem('tempProfilePhoto', currentPreview);
        });
    });

    // Check for stored preview when page loads
    const storedPreview = sessionStorage.getItem('tempProfilePhoto');
    if (storedPreview) {
        profilePhoto.src = storedPreview;
        sessionStorage.removeItem('tempProfilePhoto'); // Clear the stored preview
    }
});