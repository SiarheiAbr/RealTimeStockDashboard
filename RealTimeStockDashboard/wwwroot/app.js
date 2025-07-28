const connection = new signalR.HubConnectionBuilder()
    .withUrl("/stockHub")
    .build();

const previousPrices = {};
const previousTrends = {};
let firstUpdateReceived = false;

connection.on("ReceiveStockUpdate", (symbol, price) => {
    handleFirstUpdateReceived();

    const cell = document.getElementById(symbol);
    if (!cell) return;

    const priceSpan = cell.querySelector("span.price");
    const arrowSpan = cell.querySelector("span.arrow");
    if (!priceSpan || !arrowSpan) return;

    const oldPrice = previousPrices[symbol];
    const oldTrend = previousTrends[symbol];

    previousPrices[symbol] = price;

    const priceText = price.toFixed(2);
    let arrow = '';
    let trend = '';
    let directionClass = '';
    let arrowClass = '';

    if (oldPrice !== undefined) {
        const change = price - oldPrice;

        if (change > 0) {
            arrow = '▲';
            trend = 'up';
            arrowClass = 'arrow-up';
        } else if (change < 0) {
            arrow = '▼';
            trend = 'down';
            arrowClass = 'arrow-down';
        }

        // Only animate if trend direction changed
        if (trend && trend !== oldTrend) {
            directionClass = trend === 'up' ? 'price-up' : 'price-down';
        }
    }

    previousTrends[symbol] = trend;

    // Apply background flash only if trend changed
    priceSpan.classList.remove("price-up", "price-down");
    if (directionClass) {
        priceSpan.classList.add(directionClass);
        setTimeout(() => priceSpan.classList.remove(directionClass), 1200); // ⬅️ longer flash
    }

    // Update arrow
    arrowSpan.classList.remove("arrow-up", "arrow-down");
    if (arrowClass) {
        arrowSpan.classList.add(arrowClass);
    }

    if (arrowSpan.innerText !== arrow) {
        arrowSpan.innerText = arrow;
    }

    // Update price number
    if (priceSpan.innerText !== priceText) {
        priceSpan.innerText = priceText;
    }
});

function handleFirstUpdateReceived() {
    if (!firstUpdateReceived) {
        firstUpdateReceived = true;
        document.getElementById("loading-indicator").style.display = "none";
        document.getElementById("stock-widgets").style.display = "flex";
        document.getElementById("stock-widgets").style.opacity = 0;

        setTimeout(() => {
            document.getElementById("stock-widgets").style.opacity = 1;
        }, 10);
    }
}

connection.start().catch(err => console.error(err.toString()));
