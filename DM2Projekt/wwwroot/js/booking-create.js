document.addEventListener("DOMContentLoaded", () => {
    const roomSelect = document.getElementById("Booking_RoomId");
    const smartboardSelect = document.getElementById("SmartboardSelect");
    const noSmartboardMsg = document.getElementById("NoSmartboardMessage");

    const weekPicker = document.getElementById("weekPicker");
    const dayOfWeekSelect = document.getElementById("dayOfWeek");
    const timeSlotSelect = document.getElementById("timeSlot");

    const startInput = document.getElementById("Booking_StartTime");
    const endInput = document.getElementById("Booking_EndTime");

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

    function updateSmartboardOptions(roomId) {
        smartboardSelect.style.display = 'none';
        noSmartboardMsg.style.display = 'none';
        smartboardSelect.innerHTML = '';

        fetch(`?handler=SmartboardsByRoom&roomId=${roomId}`)
            .then(res => res.json())
            .then(data => {
                if (!data.length) {
                    noSmartboardMsg.style.display = 'block';
                    return;
                }

                smartboardSelect.style.display = 'block';
                smartboardSelect.appendChild(new Option("-- I don't want a smartboard --", ""));

                data.forEach(sb => {
                    smartboardSelect.appendChild(new Option(sb.display, sb.smartboardId));
                });
            });
    }

    function updateAvailableTimeSlots() {
        const roomId = roomSelect.value;
        const selectedDate = getSelectedDate();

        if (!roomId || !selectedDate) {
            timeSlotSelect.innerHTML = '<option value="">-- Select Room + Day --</option>';
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
            });
    }

    function updateHiddenTimeInputs(selectedStart) {
        if (!selectedStart) return;

        const startDate = new Date(selectedStart);
        const endDate = new Date(startDate.getTime() + 2 * 60 * 60 * 1000);

        startInput.value = startDate.toISOString();
        endInput.value = endDate.toISOString();
    }

    // Event Listeners
    roomSelect.addEventListener("change", () => {
        updateSmartboardOptions(roomSelect.value);
        updateAvailableTimeSlots();
    });

    weekPicker.addEventListener("change", updateAvailableTimeSlots);
    dayOfWeekSelect.addEventListener("change", updateAvailableTimeSlots);

    timeSlotSelect.addEventListener("change", () => {
        updateHiddenTimeInputs(timeSlotSelect.value);
    });

    // Pre-fill if already selected
    if (roomSelect.value) {
        roomSelect.dispatchEvent(new Event("change"));
    }
});
