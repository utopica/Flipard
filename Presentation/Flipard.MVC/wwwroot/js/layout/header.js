$(document).ready(function() {
    const $searchInput = $('#search-input'); 
    const $searchResults = $('#search-results'); 

    $searchInput.on('keyup', function() {
        const query = $searchInput.val(); 
        if (query.length > 2) {
            $.ajax({
                url: '/Home/SearchDecks',
                type: 'GET',
                data: { query: query },
                success: function(data) {
                    $searchResults.empty().show();
                    if (data.length > 0) {
                        data.forEach(function(deck) {
                            $searchResults.append('<li class="dropdown-item"><a href="/Flashcards/Index/' + deck.id + '">' + deck.name + '</a></li>');
                        });
                    } else {
                        $searchResults.append('<li class="dropdown-item">No results found</li>');
                    }
                },
                error: function() {
                    $searchResults.hide(); 
                }
            });
        } else {
            $searchResults.hide(); 
        }
    });

    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-form').length) {
            $searchResults.hide(); 
        }
    });
});
