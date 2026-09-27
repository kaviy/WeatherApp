(() => {
    const API_URL = "/api/weather";

    const loadingIndicator = document.getElementById("loadingIndicator");
    const errorBanner = document.getElementById("errorBanner");
    const table = document.getElementById("weatherTable");
    const tableBody = document.getElementById("weatherTableBody");
    const detailPanel = document.getElementById("detailPanel");
    const minTempFilter = document.getElementById("minTempFilter");
    const refreshButton = document.getElementById("refreshButton");
    const statusBadge = document.getElementById("statusBadge");

    /** @type {Array<object>} */
    let allRecords = [];
    let sortState = { key: "date", direction: "asc" };

    async function loadWeather() {
        setLoading(true);
        hideError();
        table.hidden = true;
        detailPanel.hidden = true;

        try {
            const response = await fetch(API_URL, { headers: { Accept: "application/json" } });

            if (!response.ok) {
                throw new Error(`Server responded with HTTP ${response.status}`);
            }

            const data = await response.json();
            allRecords = normalizeKeys(data);
            statusBadge.textContent = `${allRecords.length} date(s) loaded`;
            renderTable();
        } catch (err) {
            showError(
                `Could not load weather data: ${err instanceof Error ? err.message : String(err)}. ` +
                "Check that the backend is running and try Refresh."
            );
        } finally {
            setLoading(false);
        }
    }

    // ASP.NET's default JSON casing is camelCase; normalize defensively in case that changes.
    function normalizeKeys(records) {
        return records.map((r) => ({
            rawInput: r.rawInput ?? r.RawInput ?? "",
            date: r.date ?? r.Date ?? null,
            minTemperature: r.minTemperature ?? r.MinTemperature ?? null,
            maxTemperature: r.maxTemperature ?? r.MaxTemperature ?? null,
            precipitationSum: r.precipitationSum ?? r.PrecipitationSum ?? null,
            status: r.status ?? r.Status ?? "Unknown",
            errorMessage: r.errorMessage ?? r.ErrorMessage ?? null
        }));
    }

    function setLoading(isLoading) {
        loadingIndicator.hidden = !isLoading;
    }

    function showError(message) {
        errorBanner.textContent = message;
        errorBanner.hidden = false;
    }

    function hideError() {
        errorBanner.hidden = true;
        errorBanner.textContent = "";
    }

    function getFilteredSortedRecords() {
        const threshold = minTempFilter.value.trim() === "" ? null : parseFloat(minTempFilter.value);

        let rows = allRecords;
        if (threshold !== null && !Number.isNaN(threshold)) {
            rows = rows.filter((r) => typeof r.maxTemperature === "number" && r.maxTemperature >= threshold);
        }

        const { key, direction } = sortState;
        const factor = direction === "asc" ? 1 : -1;

        return [...rows].sort((a, b) => {
            const av = a[key];
            const bv = b[key];

            // Push rows with missing values (invalid/error dates) to the bottom regardless of direction.
            if (av === null || av === undefined) return 1;
            if (bv === null || bv === undefined) return -1;

            if (av < bv) return -1 * factor;
            if (av > bv) return 1 * factor;
            return 0;
        });
    }

    function renderTable() {
        const rows = getFilteredSortedRecords();
        tableBody.innerHTML = "";

        rows.forEach((record, index) => {
            const tr = document.createElement("tr");
            tr.dataset.index = String(index);

            tr.appendChild(cell(record.date ?? record.rawInput));
            tr.appendChild(cell(formatNumber(record.minTemperature)));
            tr.appendChild(cell(formatNumber(record.maxTemperature)));
            tr.appendChild(cell(formatNumber(record.precipitationSum)));
            tr.appendChild(statusCell(record.status));

            tr.addEventListener("click", () => showDetail(record));
            tableBody.appendChild(tr);
        });

        updateSortArrows();
        table.hidden = false;
    }

    function cell(text) {
        const td = document.createElement("td");
        td.textContent = text ?? "\u2014";
        return td;
    }

    function statusCell(status) {
        const td = document.createElement("td");
        td.textContent = status;
        td.classList.add(`row-status-${String(status).toLowerCase()}`);
        return td;
    }

    function formatNumber(value) {
        return typeof value === "number" ? value.toFixed(1) : null;
    }

    function showDetail(record) {
        detailPanel.hidden = false;
        detailPanel.innerHTML = `
            <h2>Details</h2>
            <dl>
                <dt>Raw input</dt><dd>${escapeHtml(record.rawInput)}</dd>
                <dt>Normalized date</dt><dd>${escapeHtml(record.date ?? "n/a")}</dd>
                <dt>Min temperature</dt><dd>${escapeHtml(formatNumber(record.minTemperature) ?? "n/a")} &deg;C</dd>
                <dt>Max temperature</dt><dd>${escapeHtml(formatNumber(record.maxTemperature) ?? "n/a")} &deg;C</dd>
                <dt>Precipitation</dt><dd>${escapeHtml(formatNumber(record.precipitationSum) ?? "n/a")} mm</dd>
                <dt>Status</dt><dd>${escapeHtml(record.status)}</dd>
                <dt>Error message</dt><dd>${escapeHtml(record.errorMessage ?? "\u2014")}</dd>
            </dl>
        `;
    }

    function escapeHtml(value) {
        const div = document.createElement("div");
        div.textContent = String(value);
        return div.innerHTML;
    }

    function updateSortArrows() {
        document.querySelectorAll("th.sortable").forEach((th) => {
            const arrow = th.querySelector(".sort-arrow");
            if (th.dataset.sortKey === sortState.key) {
                arrow.textContent = sortState.direction === "asc" ? "\u25B2" : "\u25BC";
            } else {
                arrow.textContent = "";
            }
        });
    }

    document.querySelectorAll("th.sortable").forEach((th) => {
        th.addEventListener("click", () => {
            const key = th.dataset.sortKey;
            if (sortState.key === key) {
                sortState.direction = sortState.direction === "asc" ? "desc" : "asc";
            } else {
                sortState = { key, direction: "asc" };
            }
            renderTable();
        });
    });

    minTempFilter.addEventListener("input", renderTable);
    refreshButton.addEventListener("click", loadWeather);

    loadWeather();
})();
