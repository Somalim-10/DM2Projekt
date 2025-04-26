document.addEventListener("DOMContentLoaded", () => {
    // get all needed HTML elements
    const roomSelect = document.getElementById("Booking_RoomId");
    const weekPicker = document.getElementById("weekPicker");
    const dayOfWeekSelect = document.getElementById("dayOfWeek");
    const timeSlotSelect = document.getElementById("timeSlot");

    const startInput = document.getElementById("Booking_StartTime");
    const endInput = document.getElementById("Booking_EndTime");
    const smartboardCheckboxContainer = document.getElementById("smartboardCheckboxContainer");
    const smartboardCheckbox = document.getElementById("Booking_UsesSmartboard");

    let isClassroom = false; // save room type here after first fetch

    // figure out selected date (from week picker + day picker)
    function getSelectedDate() {
        const week = weekPicker.value;
        const day = parseInt(dayOfWeekSelect.value);
        if (!week || isNaN(day)) return null;

        const [year, weekNum] = week.split("-W").map(Number);
        const janFirst = new Date(year, 0, 1);
        const daysOffset = ((janFirst.getDay() + 6) % 7);
        const monday = new Date(janFirst);
        monday.setDate(janFirst.getDate() - daysOffset + ((weekNum - 1) * 7) + 1);
        monday.setDate(monday.getDate() + (day - 1));
        return monday;
    }

    // update time slots when room, week, or day changes
    function updateAvailableTimeSlots() {
        const roomId = roomSelect.value;
        const selectedDate = getSelectedDate();

        if (!roomId || !selectedDate) {
            smartboardCheckboxContainer.style.display = "none";
            smartboardCheckbox.checked = false;
            return;
        }

        const isoDate = selectedDate.toISOString().split("T")[0];

        fetch(`?handler=AvailableTimeSlots&roomId=${roomId}&date=${isoDate}`)
            .then(res => res.json())
            .then(data => {
                timeSlotSelect.innerHTML = '';

                if (!data.length) {
                    timeSlotSelect.innerHTML = '<option value="">No available slots</option>';
                    return;
                }

                data.forEach(slot => {
                    const option = new Option(`${slot.start} - ${slot.end}`, slot.value);
                    timeSlotSelect.appendChild(option);
                });

                if (isClassroom) {
                    smartboardCheckboxContainer.style.display = "block";
                    smartboardCheckbox.checked = false;
                } else {
                    smartboardCheckboxContainer.style.display = "none";
                    smartboardCheckbox.checked = false;
                }
            });
    }

    // check if smartboard is already booked
    function checkSmartboardAvailability() {
        const roomId = roomSelect.value;
        const selectedSlot = timeSlotSelect.value;
        if (!selectedSlot) return;

        const startTime = new Date(selectedSlot);
        const endTime = new Date(startTime.getTime() + 2 * 60 * 60 * 1000);

        fetch(`?handler=SmartboardCheck&roomId=${roomId}&start=${startTime.toISOString()}&end=${endTime.toISOString()}`)
            .then(res => res.json())
            .then(isAlreadyBooked => {
                smartboardCheckbox.disabled = isAlreadyBooked;
                if (isAlreadyBooked) smartboardCheckbox.checked = false;
            });
    }

    // put selected time slot into hidden fields (start + end time)
    function updateHiddenTimeInputs(selectedStart) {
        if (!selectedStart) return;

        const startDate = new Date(selectedStart);
        const endDate = new Date(startDate.getTime() + 2 * 60 * 60 * 1000);

        startInput.value = startDate.toISOString();
        endInput.value = endDate.toISOString();

        const selectedTimeSlotInput = document.querySelector('input[name="SelectedTimeSlot"]');
        if (selectedTimeSlotInput) {
            selectedTimeSlotInput.value = startDate.toISOString();
        }
    }

    // event listeners to trigger when user changes stuff
    roomSelect.addEventListener("change", () => {
        const roomId = roomSelect.value;
        if (!roomId) {
            smartboardCheckboxContainer.style.display = "none";
            smartboardCheckbox.checked = false;
            return;
        }

        fetch(`?handler=RoomType&roomId=${roomId}`)
            .then(res => res.json())
            .then(room => {
                isClassroom = room.roomType === "Classroom";
                updateAvailableTimeSlots();
            });
    });

    weekPicker.addEventListener("change", updateAvailableTimeSlots);
    dayOfWeekSelect.addEventListener("change", updateAvailableTimeSlots);

    timeSlotSelect.addEventListener("change", () => {
        updateHiddenTimeInputs(timeSlotSelect.value);
        checkSmartboardAvailability();
    });

    // if room already picked, auto load time slots
    if (roomSelect.value) {
        roomSelect.dispatchEvent(new Event("change"));
    }
});
