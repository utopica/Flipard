function navigateToDeck(deckId, isReadOnly) {
    window.location.href = '/Flashcards/Index/' + deckId + (isReadOnly ? '?isReadonly=true' : '');
}

function scrollDecks(deckListClass, direction) {
    const deckList = document.querySelector(deckListClass);
    const scrollAmount = 300;

    if (direction === 'next') {
        deckList.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    } else if (direction === 'previous') {
        deckList.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    }
}

class QuizCalendar {
    constructor() {
        this.currentDate = new Date();
        this.calendarDays = null;
        this.monthYear = null;
        this.streakCount = null;
        this.quizDays = [];
        this.changeMonth = this.changeMonth.bind(this);
    }

    async initialize() {
        try {
            // Initialize DOM elements
            this.calendarDays = document.getElementById('calendarDays');
            this.monthYear = document.getElementById('monthYear');
            this.streakCount = document.getElementById('streakCount');

            // Check if required elements exist
            if (!this.calendarDays || !this.monthYear || !this.streakCount) {
                throw new Error('Required calendar elements not found');
            }

            // Initialize navigation buttons
            const prevButton = document.getElementById('prevMonthBtn');
            const nextButton = document.getElementById('nextMonthBtn');

            if (!prevButton || !nextButton) {
                throw new Error('Navigation buttons not found');
            }

            // Add event listeners
            prevButton.addEventListener('click', () => this.changeMonth('prev'));
            nextButton.addEventListener('click', () => this.changeMonth('next'));

            // Fetch quiz attempts and load calendar
            await this.fetchQuizAttempts();
            this.loadCalendar(this.currentDate);
            this.updateStreak();
        } catch (error) {
            console.error('Calendar initialization failed:', error);
        }
    }

    async fetchQuizAttempts() {
        try {
            const response = await fetch('/api/QuizAttempts/user-attempts');
            const attempts = await response.json();
            this.quizDays = [...new Set(attempts.map(attempt =>
                new Date(attempt.attemptDate).toISOString().split('T')[0]
            ))].sort();
        } catch (error) {
            console.error('Error fetching quiz attempts:', error);
            this.quizDays = [];
        }
    }

    loadCalendar(date) {
        if (!this.calendarDays) return;

        const year = date.getFullYear();
        const month = date.getMonth();

        this.monthYear.textContent = new Date(year, month)
            .toLocaleDateString('en-US', {
                month: 'long',
                year: 'numeric'
            });

        this.calendarDays.innerHTML = '';

        // Get first day of month and total days
        const firstDayOfMonth = new Date(year, month, 1).getDay();
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        // Add empty cells for alignment
        for (let i = 0; i < firstDayOfMonth; i++) {
            const emptyDay = document.createElement('div');
            emptyDay.className = 'day empty';
            this.calendarDays.appendChild(emptyDay);
        }

        // Add calendar days
        for (let day = 1; day <= daysInMonth; day++) {
            const dayElement = document.createElement('div');
            dayElement.className = 'day';

            const dayString = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isQuizDay = this.quizDays.includes(dayString);

            if (isQuizDay) {
                dayElement.classList.add('quiz-day');
            }

            // Highlight current day
            const isToday = this.isToday(year, month, day);
            if (isToday) {
                dayElement.classList.add('today');
            }

            dayElement.textContent = day;
            this.calendarDays.appendChild(dayElement);
        }
    }
    updateStreak() {
        if (!this.streakCount || !this.quizDays.length) {
            if (this.streakCount) this.streakCount.textContent = '0';
            return;
        }

        const viewedYear = this.currentDate.getFullYear();
        const viewedMonth = this.currentDate.getMonth();

        // Filter quizDays to include only those in the current month and year
        const filteredQuizDays = this.quizDays.filter(day => {
            const quizDate = new Date(day);
            return quizDate.getFullYear() === viewedYear && quizDate.getMonth() === viewedMonth;
        });

        if (!filteredQuizDays.length) {
            this.streakCount.textContent = '0';
            return;
        }

        // Convert filtered dates to timestamps for easier comparison
        const timestamps = filteredQuizDays.map(day => new Date(day).getTime());
        timestamps.sort((a, b) => b - a); // Sort in descending order

        let currentStreak = 0;
        let previousDate = null;

        // Calculate consecutive streaks within the current month
        for (let i = 0; i < timestamps.length; i++) {
            const currentDate = new Date(timestamps[i]);

            if (!previousDate) {
                currentStreak = 1;
                previousDate = currentDate;
                continue;
            }

            const dayDifference = Math.floor((previousDate.getTime() - currentDate.getTime()) / (1000 * 60 * 60 * 24));

            if (dayDifference === 1) {
                // Days are consecutive
                currentStreak++;
                previousDate = currentDate;
            } else if (dayDifference === 0) {
                // Same day, continue checking
                previousDate = currentDate;
            } else {
                // Streak is broken
                break;
            }
        }

        this.streakCount.textContent = currentStreak.toString();
    }


    isToday(year, month, day) {
        const today = new Date();
        return year === today.getFullYear() &&
            month === today.getMonth() &&
            day === today.getDate();
    }

    changeMonth(direction) {
        const newDate = new Date(this.currentDate);
        if (direction === 'next') {
            newDate.setMonth(newDate.getMonth() + 1);
        } else {
            newDate.setMonth(newDate.getMonth() - 1);
        }
        this.currentDate = newDate;
        this.loadCalendar(this.currentDate);
        this.updateStreak();
    }
}

// Initialize calendar after DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    const quizCalendar = new QuizCalendar();
    quizCalendar.initialize();
    // Make calendar instance available globally
    window.quizCalendar = quizCalendar;
});


/* Quiz Of the Day */
function flipCard() {
    const card = document.getElementById('quiz-card');
    card.classList.toggle('flipped');
}

// Optional: Add keyboard support for flipping
document.addEventListener('keydown', function(e) {
    if (e.key === ' ' || e.key === 'Enter') { // Space or Enter key
        flipCard();
    }
});