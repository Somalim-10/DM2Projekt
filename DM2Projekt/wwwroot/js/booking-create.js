document.addEventListener("DOMContentLoaded", () => {
    const getEl = id => document.getElementById(id);

    const roomSelect = getEl("Booking_RoomId");
    const weekPicker = getEl("weekPicker");
    const dayOfWeekSelect = getEl("dayOfWeek");
    const timeSlotSelect = getEl("timeSlot");

    const startInput = getEl("Booking_StartTime");
    const endInput = getEl("Booking_EndTime");
    const smartboardCheckboxContainer = getEl("smartboardCheckboxContainer");
    const smartboardCheckbox = getEl("Booking_UsesSmartboard");

    const selectedTimeSlotInput = document.querySelector('input[name="SelectedTimeSlot"]');
    const selectedWeekInput = getEl("SelectedWeek");
    const selectedDayInput = getEl("SelectedDay");

    const form = document.querySelector("form");
    const imgInput = getEl("Room_ImageUrl");

    let isClassroom = false;
    let selectedRoomId = null;

    const HOUR = 60 * 60 * 1000;
    const SLOT_LENGTH = 2 * HOUR;

    function getSelectedDate() {
        const week = weekPicker.value;
        const day = parseInt(dayOfWeekSelect.value);
        if (!week || isNaN(day)) return null;

        const [year, weekNum] = week.split("-W").map(Number);
        const janFirst = new Date(year, 0, 1);
        const offset = (janFirst.getDay() + 6) % 7;
        const monday = new Date(janFirst);
        monday.setDate(janFirst.getDate() - offset + (weekNum - 1) * 7 + 1);
        monday.setDate(monday.getDate() + (day - 1));
        return monday;
    }

    function updateAvailableTimeSlots() {
        const date = getSelectedDate();
        if (!selectedRoomId || !date) {
            hideSmartboard();
            return;
        }

        const isoDate = date.toISOString().split("T")[0];

        fetch(`Create?handler=AvailableTimeSlots&roomId=${selectedRoomId}&date=${isoDate}`)
            .then(res => res.json())
            .then(slots => {
                timeSlotSelect.innerHTML = "";

                if (!slots.length) {
                    timeSlotSelect.innerHTML = '<option value="">No available slots</option>';
                    return;
                }

                slots.forEach(slot => {
                    const option = new Option(`${slot.start} - ${slot.end}`, slot.value);
                    timeSlotSelect.appendChild(option);
                });

                if (timeSlotSelect.value) {
                    updateHiddenTimeInputs(timeSlotSelect.value);
                    checkSmartboardAvailability();
                }

                if (isClassroom) {
                    showSmartboard();
                } else {
                    hideSmartboard();
                }
            });
    }

    function updateHiddenTimeInputs(start) {
        if (!start) return;

        const startDate = new Date(start);
        const endDate = new Date(startDate.getTime() + SLOT_LENGTH);

        startInput.value = startDate.toISOString();
        endInput.value = endDate.toISOString();

        if (selectedTimeSlotInput) {
            selectedTimeSlotInput.value = startDate.toISOString();
        }
    }

    function updateHiddenWeekDayInputs() {
        if (selectedWeekInput) selectedWeekInput.value = weekPicker.value;
        if (selectedDayInput) selectedDayInput.value = dayOfWeekSelect.value;
    }

    function checkSmartboardAvailability() {
        if (!isClassroom || !timeSlotSelect.value) return;

        const start = new Date(timeSlotSelect.value);
        const end = new Date(start.getTime() + SLOT_LENGTH);

        fetch(`Create?handler=SmartboardCheck&roomId=${selectedRoomId}&start=${start.toISOString()}&end=${end.toISOString()}`)
            .then(res => res.json())
            .then(isBooked => {
                smartboardCheckbox.disabled = isBooked;
                if (isBooked) smartboardCheckbox.checked = false;
            });
    }

    function hideSmartboard() {
        smartboardCheckboxContainer.style.display = "none";
        smartboardCheckbox.checked = false;
    }

    function showSmartboard() {
        smartboardCheckboxContainer.style.display = "block";
        smartboardCheckbox.disabled = true;
    }

    function handleRoomChange(roomId) {
        selectedRoomId = roomId;

        if (!roomId) {
            hideSmartboard();
            return;
        }

        // Need to know if the selected room is a classroom
        fetch(`Create?handler=RoomType&roomId=${roomId}`)
            .then(res => res.json())
            .then(room => {
                isClassroom = room.roomType === "Classroom";
                updateAvailableTimeSlots();
            });
    }

    function attachListeners() {
        roomSelect.addEventListener("change", () => {
            handleRoomChange(roomSelect.value);
        });

        weekPicker.addEventListener("change", () => {
            updateAvailableTimeSlots();
            updateHiddenWeekDayInputs();
        });

        dayOfWeekSelect.addEventListener("change", () => {
            updateAvailableTimeSlots();
            updateHiddenWeekDayInputs();
        });

        timeSlotSelect.addEventListener("change", () => {
            updateHiddenTimeInputs(timeSlotSelect.value);
            checkSmartboardAvailability();
        });

        form.addEventListener("submit", () => {
            updateHiddenTimeInputs(timeSlotSelect.value);
            updateHiddenWeekDayInputs();
        });

        if (imgInput) {
            imgInput.addEventListener("input", e => {
                const url = e.target.value;
                const regex = /\.(jpg|jpeg|png|gif)$/i;
                if (!regex.test(url)) {
                    alert("Please enter a valid image URL (e.g., ends with .jpg, .png).");
                }
            });
        }
    }

    // This needs to run right away if the page is loading with a pre-selected room
    if (roomSelect.value) {
        handleRoomChange(roomSelect.value);
    }

    attachListeners();
});
