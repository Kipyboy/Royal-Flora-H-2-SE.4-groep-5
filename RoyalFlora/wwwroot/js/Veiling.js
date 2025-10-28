(() =>
{
    const DEFAULT_MS = 5 * 60 * 1000;
    const STORAGE_KEY = "veiling_end_ts";
    const DISPLAY_ID = "veiling-timer";
    let endTs = null;
    let tickId = null;

    function now()
    {
        return Date.now();
    }

    function readEndFromStorage()
    {
        const v = localStorage.getItem(STORAGE_KEY);
        const n = v ? parseInt(v, 10) : NaN;
        if (!isFinite(n)) return null;
        return n;
    }

    function writeEndToStorage(ts)
    {
        localStorage.setItem(STORAGE_KEY, String(ts));
    }

    function formatMs(ms)
    {
        if (ms <= 0) return "00:00";
        const totalSec = Math.floor(ms / 1000);
        const min = Math.floor(totalSec / 60);
        const sec = totalSec % 60;
        return String(min).padStart(2, "0") + ":" + String(sec).padStart(2, "0");
    }

    function updateDisplay()
    {
        const el = document.getElementById(DISPLAY_ID);
        const remaining = Math.max(0, (endTs || 0) - now());
        el.textContent = formatMs(remaining);
        if (remaining <= 0)
        {
            stopTick();
        }
    }

    function startTick()
    {
        if (tickId) return;
        updateDisplay();
        tickId = setInterval(updateDisplay, 250);
        document.addEventListener("visibilitychange", updateDisplay);
    }

    function stopTick()
    {
        if (tickId)
        {
            clearInterval(tickId);
            tickId = null;
        }
        document.removeEventListener("visibilitychange", updateDisplay);
    }

    function startIfNeeded()
    {
        const stored = readEndFromStorage();
        if (stored && stored > now())
        {
            endTs = stored;
        }
        else
        {
            endTs = now() + DEFAULT_MS;
            writeEndToStorage(endTs);
        }
        startTick();
    }

    window.addEventListener("storage", (e) =>
    {
        if (e.key !== STORAGE_KEY) return;
        const stored = readEndFromStorage();
        if (!stored)
        {
            endTs = null;
            updateDisplay();
            return;
        }
        endTs = stored;
        updateDisplay();
    });

    window.VeilingTimer = {
        reset: () =>
        {
            endTs = now() + DEFAULT_MS;
            writeEndToStorage(endTs);
            startTick();
        },
        stop: () =>
        {
            stopTick();
        }
    };

    document.addEventListener("DOMContentLoaded", () =>
    {
        startIfNeeded();
        updateClock();
        setInterval(updateClock, 1000);

        const photoStrip = document.getElementById("photo-strip");
        const viewer = document.getElementById("viewer");
        
        /*even tijdelijke fotos*/
        const photos = [
            "https://picsum.photos/id/1015/1155/615",
            "https://picsum.photos/id/1016/600/400",
            "https://picsum.photos/id/1018/600/400",
            "https://picsum.photos/id/1020/600/400",
            "https://picsum.photos/id/1024/600/400",
            "https://picsum.photos/id/1025/600/400",
            "https://picsum.photos/id/1035/600/400"
        ];

        photos.forEach((url) =>
        {
            const img = document.createElement("img");
            img.src = url;
            img.alt = "Foto";
            img.addEventListener("click", () => showInViewer(url));
            photoStrip.appendChild(img);
        });

        function showInViewer(url)
        {
            viewer.innerHTML = "";
            const img = document.createElement("img");
            img.src = url;
            viewer.appendChild(img);
            viewer.classList.remove("hidden");
        }
    });


    function updateClock()
    {
        const now = new Date();
        const tijd = now.toLocaleTimeString("nl-NL",
        {
            hour12: false
        });
        document.getElementById("huidige-tijd").textContent = tijd;
    }

    setInterval(updateClock, 1000);
    updateClock();

})();