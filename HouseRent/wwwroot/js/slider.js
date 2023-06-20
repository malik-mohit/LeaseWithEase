
const rangeInputBudget = document.querySelectorAll("#range-input-budget input"),
    priceInputBudget = document.querySelectorAll("#price-input-budget input"),
    rangeBudget = document.querySelector("#slider-budget .progress");
let priceGapBudget = 1000;
priceInputBudget.forEach(input => {
    input.addEventListener("input", e => {
        let minPrice = parseInt(priceInputBudget[0].value),
            maxPrice = parseInt(priceInputBudget[1].value);

        if ((maxPrice - minPrice >= priceGapBudget) && maxPrice <= rangeInputBudget[1].max) {
            if (e.target.className === "input-min") {
                rangeInputBudget[0].value = minPrice;
                rangeBudget.style.left = ((minPrice / rangeInputBudget[0].max) * 100) + "%";
            } else {
                rangeInputBudget[1].value = maxPrice;
                rangeBudget.style.right = 100 - (maxPrice / rangeInputBudget[1].max) * 100 + "%";
            }
        }

    });
});
rangeInputBudget.forEach(input => {
    input.addEventListener("input", e => {
        let minVal = parseInt(rangeInputBudget[0].value),
            maxVal = parseInt(rangeInputBudget[1].value);
        if ((maxVal - minVal) < priceGapBudget) {
            if (e.target.className === "range-min") {
                rangeInputBudget[0].value = maxVal - priceGapBudget;
            } else {
                rangeInputBudget[1].value = minVal + priceGapBudget;
            }
        } else {
            priceInputBudget[0].value = minVal;
            priceInputBudget[1].value = maxVal;
            rangeBudget.style.left = ((minVal / rangeInputBudget[0].max) * 100) + "%";
            rangeBudget.style.right = 100 - (maxVal / rangeInputBudget[1].max) * 100 + "%";
        }
    });
});



const rangeInputArea = document.querySelectorAll("#range-input-area input"),
    priceInputArea = document.querySelectorAll("#price-input-area input"),
    rangeArea = document.querySelector("#slider-area .progress");
let priceGapArea = 1000;
priceInputArea.forEach(input => {
    input.addEventListener("input", e => {
        let minPrice = parseInt(priceInputArea[0].value),
            maxPrice = parseInt(priceInputArea[1].value);

        if ((maxPrice - minPrice >= priceGapArea) && maxPrice <= rangeInputArea[1].max) {
            if (e.target.className === "input-min") {
                rangeInputArea[0].value = minPrice;
                rangeArea.style.left = ((minPrice / rangeInputArea[0].max) * 100) + "%";
            } else {
                rangeInputArea[1].value = maxPrice;
                rangeArea.style.right = 100 - (maxPrice / rangeInputArea[1].max) * 100 + "%";
            }
        }

    });
});
rangeInputArea.forEach(input => {
    input.addEventListener("input", e => {
        let minVal = parseInt(rangeInputArea[0].value),
            maxVal = parseInt(rangeInputArea[1].value);
        if ((maxVal - minVal) < priceGapArea) {
            if (e.target.className === "range-min") {
                rangeInputArea[0].value = maxVal - priceGapArea;
            } else {
                rangeInputArea[1].value = minVal + priceGapArea;
            }
        } else {
            priceInputArea[0].value = minVal;
            priceInputArea[1].value = maxVal;
            rangeArea.style.left = ((minVal / rangeInputArea[0].max) * 100) + "%";
            rangeArea.style.right = 100 - (maxVal / rangeInputArea[1].max) * 100 + "%";
        }
    });
});