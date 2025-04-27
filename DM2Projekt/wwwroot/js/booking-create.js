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

    const selectedTimeSlotInput = document.querySelector('input[name="SelectedTimeSlot"]');
    const selectedWeekInput = document.getElementById("SelectedWeek");
    const selectedDayInput = document.getElementById("SelectedDay");

    const form = document.querySelector("form");

    let isClassroom = false; // true if room is a classroom
    let selectedRoomId = null; // remember selected room id

    // calculate selected full date (based on week + day)
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

    // update time slot dropdown
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
                    smartboardCheckbox.disabled = true; // disable until user picks a slot
                } else {
                    smartboardCheckboxContainer.style.display = "none";
                    smartboardCheckbox.checked = false;
                }
            });
    }

    // check if smartboard is already booked for selected slot
    function checkSmartboardAvailability() {
        const roomId = selectedRoomId;
        const selectedSlot = timeSlotSelect.value;
        if (!selectedSlot || !isClassroom) return;

        const startTime = new Date(selectedSlot);
        const endTime = new Date(startTime.getTime() + 2 * 60 * 60 * 1000); // +2h

        fetch(`Create?handler=SmartboardCheck&roomId=${roomId}&start=${startTime.toISOString()}&end=${endTime.toISOString()}`)
            .then(res => res.json())
            .then(isAlreadyBooked => {
                smartboardCheckbox.disabled = isAlreadyBooked;
                if (isAlreadyBooked) smartboardCheckbox.checked = false;
            });
    }

    // fill hidden time slot fields
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

    // fill hidden week/day fields
    function updateHiddenWeekDayInputs() {
        if (selectedWeekInput) selectedWeekInput.value = weekPicker.value;
        if (selectedDayInput) selectedDayInput.value = dayOfWeekSelect.value;
    }

    // when room is changed
    roomSelect.addEventListener("change", () => {
        const roomId = roomSelect.value;
        if (!roomId) {
            smartboardCheckboxContainer.style.display = "none";
            smartboardCheckbox.checked = false;
            selectedRoomId = null;
            return;
        }

        selectedRoomId = roomId; // remember the selected room
        fetch(`Create?handler=RoomType&roomId=${roomId}`)
            .then(res => res.json())
            .then(room => {
                isClassroom = room.roomType === "Classroom";
                updateAvailableTimeSlots();
            });
    });

    // when week changes
    weekPicker.addEventListener("change", () => {
        updateAvailableTimeSlots();
        updateHiddenWeekDayInputs();
    });

    // when day changes
    dayOfWeekSelect.addEventListener("change", () => {
        updateAvailableTimeSlots();
        updateHiddenWeekDayInputs();
    });

    // when time slot changes
    timeSlotSelect.addEventListener("change", () => {
        updateHiddenTimeInputs(timeSlotSelect.value);
        checkSmartboardAvailability(); // also check smartboard free or not
    });

    // before form submit, update hidden fields
    form.addEventListener("submit", () => {
        updateHiddenTimeInputs(timeSlotSelect.value);
        updateHiddenWeekDayInputs();
    });

    // if room is pre-selected, trigger load
    if (roomSelect.value) {
        roomSelect.dispatchEvent(new Event("change"));
    }
});
