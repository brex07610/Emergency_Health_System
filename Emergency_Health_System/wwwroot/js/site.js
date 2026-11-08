(() => {
    const panel = document.querySelector("[data-tracking-url]");
    if (!panel) {
        return;
    }

    const status = panel.querySelector("[data-tracking-status]");
    const eta = panel.querySelector("[data-tracking-eta]");
    const vehicle = panel.querySelector("[data-tracking-vehicle]");
    const location = panel.querySelector("[data-tracking-location]");
    const checked = panel.querySelector("[data-tracking-checked]");
    const pending = panel.querySelector("[data-pending-message]");
    const error = panel.querySelector("[data-tracking-error]");
    const dateFormat = new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short"
    });

    const displayDate = value => value ? dateFormat.format(new Date(value)) : "Not available yet";

    const refresh = async () => {
        try {
            const response = await fetch(panel.dataset.trackingUrl, {
                headers: { Accept: "application/json" },
                cache: "no-store",
                credentials: "same-origin"
            });
            if (!response.ok) {
                throw new Error(`Tracking update failed (${response.status}).`);
            }

            const data = await response.json();
            status.textContent = data.status;
            eta.textContent = displayDate(data.estimatedArrivalUtc);
            vehicle.textContent = data.vehicleNumber || "Not assigned yet";
            location.textContent = data.vehicleLocation || "Not available yet";
            checked.dateTime = data.checkedAtUtc;
            checked.textContent = displayDate(data.checkedAtUtc);
            if (pending) {
                pending.classList.toggle("d-none", data.status !== "Pending");
            }
            error.classList.add("d-none");

            if (data.isTerminal && intervalId !== null) {
                window.clearInterval(intervalId);
                intervalId = null;
            }
        } catch {
            error.classList.remove("d-none");
        }
    };

    let intervalId = null;
    refresh();
    intervalId = window.setInterval(refresh, 15000);
})();
