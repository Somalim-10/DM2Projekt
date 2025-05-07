document.addEventListener("DOMContentLoaded", () => {
    const roomSelect = document.getElementById("Booking_RoomId");
    const weekPicker = document.getElementById("weekPicker");
    const dayOfWeekSelect = document.getElementById("dayOfWeek");
    const timeSlotSelect = document.getElementById("timeSlot");

    const startInput = document.getElementById("Booking_StartTime");
    const endInput = document.getElementById("Booking_EndTime");
    const smartboardCheckboxContainer = document.getElementById("smartboardCheckboxContainer");
    const smartboardCheckbox = document.getElementById("Booking_UsesSmartboard");

    const selectedTimeSlotInput = document.querySelector('input[name="SelectedTimeSlot"]');
    const selectedWeekInput = document.getElementById("SelectedWeek");
    const selectedDayInput = document.getElementById("SelectedDay");

    const form = document.querySelector("form");

    let isClassroom = false;
    let selectedRoomId = null;

    function getSelectedDate() {
        const week = weekPicker.value;
        const day = parseInt(dayOfWeekSelect.value);
        if (!week || isNaN(day)) return null;

        const [year, weekNum] = week.split("-W").map(Number);
        const janFirst = new Date(year, 0, 1);
        const daysOffset = (janFirst.getDay() + 6) % 7;
        const monday = new Date(janFirst);
        monday.setDate(janFirst.getDate() - daysOffset + ((weekNum - 1) * 7) + 1);
        monday.setDate(monday.getDate() + (day - 1));
        return monday;
    }

    function updateAvailableTimeSlots() {
        const roomId = selectedRoomId;
        const selectedDate = getSelectedDate();
        if (!roomId || !selectedDate) {
            smartboardCheckboxContainer.style.display = "none";
            smartboardCheckbox.checked = false;
            return;
        }

        const isoDate = selectedDate.toISOString().split("T")[0];

        fetch(`Create?handler=AvailableTimeSlots&roomId=${roomId}&date=${isoDate}`)
            .then(res => res.json())
            .then(data => {
                timeSlotSelect.innerHTML = "";

                if (!data.length) {
                    timeSlotSelect.innerHTML = '<option value="">No available slots</option>';
                    return;
                }

                data.forEach(slot => {
                    const option = new Option(`${slot.start} - ${slot.end}`, slot.value);
                    timeSlotSelect.appendChild(option);
                });

                if (timeSlotSelect.value) {
                    updateHiddenTimeInputs(timeSlotSelect.value);
                    checkSmartboardAvailability();
                }

                if (isClassroom) {
                    smartboardCheckboxContainer.style.display = "block";
                    smartboardCheckbox.disabled = true;
                } else {
                    smartboardCheckboxContainer.style.display = "none";
                    smartboardCheckbox.checked = false;
                }
            });
    }

    function updateHiddenTimeInputs(selectedStart) {
        if (!selectedStart) return;

        const startDate = new Date(selectedStart);
        const endDate = new Date(startDate.getTime() + 2 * 60 * 60 * 1000);

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

        const startTime = new Date(timeSlotSelect.value);
        const endTime = new Date(startTime.getTime() + 2 * 60 * 60 * 1000);

        fetch(`Create?handler=SmartboardCheck&roomId=${selectedRoomId}&start=${startTime.toISOString()}&end=${endTime.toISOString()}`)
            .then(res => res.json())
            .then(isBooked => {
                smartboardCheckbox.disabled = isBooked;
                if (isBooked) smartboardCheckbox.checked = false;
            });
    }

    // Event listeners
    roomSelect.addEventListener("change", () => {
        const roomId = roomSelect.value;
        selectedRoomId = roomId;

        if (!roomId) {
            smartboardCheckboxContainer.style.display = "none";
            smartboardCheckbox.checked = false;
            return;
        }

        fetch(`Create?handler=RoomType&roomId=${roomId}`)
            .then(res => res.json())
            .then(room => {
                isClassroom = room.roomType === "Classroom";
                updateAvailableTimeSlots();
            });
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

    // Preload if room is already selected
    if (roomSelect.value) {
        roomSelect.dispatchEvent(new Event("change"));
    }

    // Quick validation for image URL input (optional, but present in original)
    const imgInput = document.getElementById("Room_ImageUrl");
    if (imgInput) {
        imgInput.addEventListener("input", function (e) {
            const url = e.target.value;
            const regex = /\.(jpg|jpeg|png|gif)$/i;
            if (!regex.test(url)) {
                alert("Please enter a valid image URL (e.g., ends with .jpg, .png).");
            }
        });
    }
});
