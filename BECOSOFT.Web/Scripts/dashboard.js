var loadedCharts = [];
var mapZoomLevel = 7;
var mapZoomCenter = { lat: 0, lon: 0 };
var weatherMapHtmlTemplate =
    `
    <!DOCTYPE html>
    <html>
    <head>
      <link rel="stylesheet" href="https://www.meteo.be/frontend/css/style.css">
    </head>
    <body>
        {HTML}
    </body>
`;

String.prototype.replaceAll = function (str1, str2, ignore) {
    return this.replace(new RegExp(str1.replace(/([\/\,\!\\\^\$\{\}\[\]\(\)\.\*\+\?\|\<\>\-\&])/g, "\\$&"), (ignore ? "gi" : "g")), (typeof (str2) == "string") ? str2.replace(/\$/g, "$$$$") : str2);
}
$(document).ready(function() {
    var periodATags = $('a[id*="period-"]');
    periodATags.click(function(e) {
        e.preventDefault();
        loadPeriodData($(this));
    });
    $('ul[id*="period-pill-"] li').on("click",
        function(e) {
            e.preventDefault();
            var id = $(this).parent().attr("id");
            var tabId = "#" + id.replace("period-pill-", "period-");
            loadPeriodData($(tabId), $(this));
        });
    $('ul[id*="period-group-pill-"] li').on("click",
        function(e) {
            e.preventDefault();
            var parent = $(this).parent();
            var id = parent.attr("id");
            parent.children().each(function() {
                $(this).removeClass("active");
                $(this).find("a").first().removeClass("active");
            });
            $(this).addClass("active");
            $(this).find("a").first().addClass("active");
            var tabId = "#" + id.replace("period-group-pill-", "period-");
            var pillId = "#" + id.replace("period-group-pill-", "period-pill-");
            var activePill = $(pillId).children("li.active").first();
            loadPeriodData($(tabId), activePill);
        });
});

$(document).ready(function () {
    $(".dashboard-period-data").each(function (e) {
        resizeLabels($(this));
    });
});
$(window).on("resize", function () {
    $(".dashboard-period-data").each(function (e) {
        resizeLabels($(this));
    });
});

function resizeLabels(el) {
    const labels = el.find('span[data-labeltype]');
    const shortLabels = labels.filter('[data-labeltype="short"]')
    const normalLabels = labels.filter('[data-labeltype="normal"]')
    if (el.width() < 285) {
        shortLabels.removeClass("d-none");
        normalLabels.addClass("d-none");
    } else {
        normalLabels.removeClass("d-none");
        shortLabels.addClass("d-none");
    }
}

$(document).on('click',
    'table[data-customcollapsible=""] > tbody > tr > td > p',
    function(e) {
        const elem = $(this);
        const tr = elem.closest('tr');
        const hasSelection = tr.css("background-color") == "rgba(0, 0, 0, 0.2)";
        const table = elem.closest('table');
        handleCollapsibleTableRowBackground(table);
        if (!hasSelection) {
            tr.css("background-color", "rgba(0,0,0,0.2)");
        }
    });

function handleCollapsibleTableRowBackground(table) {
    table.find("tbody > tr:visible").each(function () {
        const trr = $(this);
        const level = parseInt(trr.data("collapsiblelevel"), 10) || 1;
        trr.css("background-color", "rgba(0,0,0," + (.05 * (level - 1)) + ")");
    });
}

$(document).on('dblclick',
    'table[data-customcollapsible=""] > tbody > tr > td > p',
    function (e) {
        const elem = $(this);
        const tr = elem.closest('tr');
        const level = parseInt(tr.data("collapsiblelevel"), 10);
        if (level === undefined || level === null || level === NaN) { return; }
        const siblings = tr.nextUntil(function() {
            return parseInt($(this).data("collapsiblelevel"), 10) <= level;
        });
        const nextLevelSiblings = siblings.filter(function(i) {
            return (parseInt($(this).data("collapsiblelevel"), 10) || 1) === level + 1;
        });
        const otherSiblings = siblings.not(nextLevelSiblings);
        const table = elem.closest('table');
        nextLevelSiblings.toggleClass("d-none");
        otherSiblings.each(function () {
            const sib = $(this);
            const sibTr = sib.closest('tr');
            const sibLevel = sibTr.data("collapsiblelevel");
            if (sibLevel - level > 1) {
                sibTr.addClass("d-none");
            } else {
                const parentTr = sibTr.prevAll(`tr[data-collapsiblelevel='${(sibLevel - 1)}']`).first();
                if (parentTr.hasClass("d-none")) {
                    sibTr.addClass("d-none");
                } else {
                    sibTr.removeClass("d-none");
                }
            }
        });
        handleCollapsibleTableRowBackground(table);
        var visibilityData = {};
        table.find('tbody > tr').each(function () {
            const row = $(this);
            const level = parseInt(row.data("collapsiblelevel"), 10) || 1;
            let levelVisibility = [];
            if (visibilityData[level.toString()] !== undefined) {
                levelVisibility = visibilityData[level.toString()];
            } else {
                visibilityData[level.toString()] = levelVisibility;
            }
            let parent = row.prevAll(`tr[data-collapsiblelevel='${(level - 1)}']:first`);
            let name = row.find('p').text().trim();
            while (parent !== undefined && parent.length !== 0) {
                let parentLevel = parseInt(parent.data("collapsiblelevel"), 10);
                name = parent.find('p').text().trim() + "_" + name;
                parent = parent.prevAll(`tr[data-collapsiblelevel='${(parentLevel - 1)}']:first`);
            }
            levelVisibility.push({ Name: name, Visible: row.hasClass('d-none') === false });
        });
        table.data('elements-visibility', JSON.stringify(visibilityData));
        if (document.selection && document.selection.empty) {
            document.selection.empty();
        } else if (window.getSelection) {
            var sel = window.getSelection();
            sel.removeAllRanges();
        }
    });

function loadElementsVisibility(table, visibilityData) {
    if (table === undefined || table === null || visibilityData === undefined) { return; }

    table.find('tbody > tr').each(function () {
        const row = $(this);
        const level = parseInt(row.data("collapsiblelevel"), 10) || 1;
        let levelVisibility = [];
        if (visibilityData[level.toString()] !== undefined) {
            levelVisibility = visibilityData[level.toString()];
        }
        let parent = row.prevAll(`tr[data-collapsiblelevel='${(level - 1)}']:first`);
        let name = row.find('p').text().trim();
        while (parent !== undefined && parent.length !== 0) {
            let parentLevel = parseInt(parent.data("collapsiblelevel"), 10);
            name = parent.find('p').text().trim() + "_" + name;
            parent = parent.prevAll(`tr[data-collapsiblelevel='${(parentLevel - 1)}']:first`);
        }
        const visibility = levelVisibility.find(e => e.Name === name);
        if (visibility === undefined || visibility === null) { return; }
        if (visibility.Visible === false) {
            row.addClass('d-none');
        } else {
            row.removeClass('d-none');
        }
    });
    table.find('tbody > tr').each(function () {
        const row = $(this);
        const level = parseInt(row.data("collapsiblelevel"), 10) || 1;
        const parentTr = row.prevAll(`tr[data-collapsiblelevel='${(level - 1)}']`).first();
        if (parentTr.hasClass("d-none")) {
            row.addClass("d-none");
        }
    });
    handleCollapsibleTableRowBackground(table);

    table.data('elements-visibility', JSON.stringify(visibilityData));
}

function loadMap(mapId) {
    var mapDiv = $(mapId);
    var mapLegendDiv = $(mapId.replace("map-", "map-legend-"));
    var mapFilterDiv = $(mapId.replace("map-", "map-filter-"));
    var statisticType = mapDiv.data("statistictype");
    var panelBody = mapDiv.closest(".panel-body");
    var panelBodyHeight = panelBody.height() - 20;
    mapDiv.height(panelBodyHeight);
    var data = { statisticType: statisticType };
    var parentBody = mapDiv.parent();
    var height = parentBody.height();
    if (mapFilterDiv.length) {
        var mapFilterSelect = mapFilterDiv.find("select");
        mapDiv.height(height - mapFilterSelect.outerHeight() - 20);
        mapFilterSelect.change(function() {
            var selectedItems = $(this).val();
            var statGroups = [];
            if (selectedItems.length > 0) {
                statGroups = selectedItems.join();
            }
            data = { statisticType: statisticType, statisticGroups: statGroups };
            fillMapData(mapId, data, mapLegendDiv);
        });
        mapFilterSelect.trigger('change');
    } else {
        mapDiv.height(height - 20);
    }
    fillMapData(mapId, data, mapLegendDiv);
}
function loadWeatherMap(id) {
    var container = $(id);
    var url = container.attr("url");
    var statisticType = container.data("statistictype");
    becosoft.ajax(url, { type: "GET" }, function(page) {
        var maxTempHtml;
        var minTempHtml;
        var precipitationHtml;

        if (statisticType === 11) {
            maxTempHtml = parseMaxTemperature(page);
            container.find(id + "-iframe1").contents().find("html").html(maxTempHtml);
            minTempHtml = parseMinTemperature(page);
            container.find(id + "-iframe2").contents().find("html").html(minTempHtml);
            precipitationHtml = parsePrecipitation(page);
            container.find(id + "-iframe3").contents().find("html").html(precipitationHtml);
        } else if (statisticType === 21) {
            maxTempHtml = parseMaxTemperature(page);
            container.find(id + "-iframe").contents().find("html").html(maxTempHtml);
        } else if (statisticType === 22) {
            minTempHtml = parseMinTemperature(page);
            container.find(id + "-iframe").contents().find("html").html(minTempHtml);
        } else if (statisticType === 23) {
            precipitationHtml = parsePrecipitation(page);
            container.find(id + "-iframe").contents().find("html").html(precipitationHtml);
        }
    });
}

function parseMaxTemperature(page) {
    var virtualDocument = document.implementation.createHTMLDocument('virtual');
    var $page = $(page, virtualDocument);
    var part = $page.find("forecast-days-item[type='temperature-max']");
    if (part.length > 0) {
        fixImages(part);
        var html = part[0];
        if (html !== "") {
            var parsedHtml = weatherMapHtmlTemplate.replace("{HTML}", html.innerHTML);
            return parsedHtml;
        }
    }

    console.log("Parsing max temperature failed");
}

function parseMinTemperature(page) {
    var virtualDocument = document.implementation.createHTMLDocument('virtual');
    var $page = $(page, virtualDocument);
    var part = $page.find("forecast-days-item[type='temperature-min']");

    if (part.length > 0) {
        fixImages(part);
        var html = part[0];
        if (html !== "") {
            var parsedHtml = weatherMapHtmlTemplate.replace("{HTML}", html.innerHTML);
            return parsedHtml;
        }
    }

    console.log("Parsing min temperature failed");
}

function parsePrecipitation(page) {
    var virtualDocument = document.implementation.createHTMLDocument('virtual');
    var $page = $(page, virtualDocument);
    var part = $page.find("forecast-days-item[type='humidity']");

    if (part.length > 0) {
        fixImages(part);
        var html = part[0];
        if (html !== "") {
            var parsedHtml = weatherMapHtmlTemplate.replace("{HTML}", html.innerHTML);
            return parsedHtml;
        }
    }

    console.log("Parsing humidity failed");
}

function fixImages(part) {
    part.find("image").each(function(index, obj) {
        var relativeImgPath = $(obj).attr("xlink:href");
        $(obj).attr("xlink:href", "https://www.meteo.be" + relativeImgPath);
    });
}

function loadPowerBI(id) {
    var reportContainer = $(id).find("div");
    var reportConfig = {
        type: 'report',
        embedUrl: reportContainer.data("embed-url"),
        accessToken: reportContainer.data("access-token"),
    };
    var reportElement = reportContainer[0];
    var report = powerbi.embed(reportElement, reportConfig);
    // height of 1100 = full page
    reportContainer.closest('div.panel-body').css("height", "850px");
}

function getMapZoom(obj, elem) {
    mapZoomLevel = obj.object.zoom;
    mapZoomCenter = obj.object.center;
}

function fillMapData(mapId, data, mapLegendDiv) {
    var mapObject = $(mapId);
    mapObject.html("");
    var url = mapObject.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var map = new OpenLayers.Map(mapId.replace("#", ""));
    map.addLayer(new OpenLayers.Layer.OSM(
        "OpenStreetMap",
        // Official OSM tileset as protocol-independent URLs
        [
            "https://a.tile.openstreetmap.org/${z}/${x}/${y}.png",
            "https://b.tile.openstreetmap.org/${z}/${x}/${y}.png",
            "https://c.tile.openstreetmap.org/${z}/${x}/${y}.png"
        ],
        null));
    map.events.register("zoomend",
        map, getMapZoom);
    map.events.register("moveend",
        map, getMapZoom);
    if (mapZoomCenter.lat === 0 || mapZoomCenter.lon === 0) {
        mapZoomCenter = { lon: 538116.679128, lat: 6550347.575926 };
    }
    var lonLat = new OpenLayers.LonLat(mapZoomCenter.lon, mapZoomCenter.lat);
    mapZoomLevel = (mapZoomLevel === 0 ? 7 : mapZoomLevel);
    var proj = new OpenLayers.Projection("EPSG:4326");
    becosoft.ajax(url, {
        data: data,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (mapData) {
            var vectorLayer = new OpenLayers.Layer.Vector("Overlay");
            var mapEntries = mapData["Entries"];
            var total = mapData["Total"];
            var entriesObject = $(mapEntries);
            var ratioDivisions = getRatioDivisions();
            entriesObject.each(function (index, entry) {
                //var point = new OpenLayers.LonLat(entry["Longtitude"], entry["Latitude"]);
                var point = new OpenLayers.Geometry.Point(entry["Longtitude"], entry["Latitude"]);
                map.setCenter(point.transform(proj, map.getProjectionObject()));
                var pointTotal = entry["Total"];
                var ratio = (pointTotal / total);
                var size = Math.sqrt(ratio / Math.PI) * 30000;
                //if (size < 30000 && total < 1000) {
                //    size = size * 30;
                //}
                var mycircle = OpenLayers.Geometry.Polygon
                    .createRegularPolygon(point, size, 40, 0);
                var featurecircle = new OpenLayers.Feature.Vector(mycircle);
                var color = getStyleFromRatio(ratio);
                featurecircle.style = {
                    strokeColor: color,
                    strokeOpacity: 1,
                    fillColor: color,
                    fillOpacity: 0.5,
                    pointRadius: size
                };
                var division = ratioDivisions[color.replace("#", "c")];
                if (division.min > pointTotal || division.min === 0) {
                    division.min = pointTotal;
                }
                if (division.max < pointTotal) {
                    division.max = pointTotal;
                }
                vectorLayer.addFeatures([featurecircle]);
            });
            map.addLayer(vectorLayer);
            createLegend(ratioDivisions, mapLegendDiv);
        }
    });
    map.setCenter(lonLat, mapZoomLevel);
}

function createLegend(ratioDivisions, mapLegend) {
    mapLegend.height(20);
    var boxes = createBox(ratioDivisions.c8B00FF.min, ratioDivisions.c8B00FF.max, "#8B00FF");
    boxes += createBox(ratioDivisions.c4B0082.min, ratioDivisions.c4B0082.max, "#4B0082");
    boxes += createBox(ratioDivisions.c0000FF.min, ratioDivisions.c0000FF.max, "#0000FF");
    boxes += createBox(ratioDivisions.c00FF00.min, ratioDivisions.c00FF00.max, "#00FF00");
    boxes += createBox(ratioDivisions.cFFFF00.min, ratioDivisions.cFFFF00.max, "#FFFF00");
    boxes += createBox(ratioDivisions.cFF7F00.min, ratioDivisions.cFF7F00.max, "#FF7F00");
    boxes += createBox(ratioDivisions.cFF0000.min, ratioDivisions.cFF0000.max, "#FF0000");
    mapLegend.html(boxes);
}

function createBox(min, max, color) {
    var description = "";
    if (min === max) {
        if (min === 0) {
            return "";
        }
        description = "" + max + "";
    } else {
        description = "" + max + " - " + min + "";
    }
    return '<div style="padding-top:2px;float:left;height:18px;"><div style="width:18px;height:18px;background-color:' + color + ';display:inline-block;vertical-align:middle"></div><span style="padding-left:2px;padding-right:6px;font-size: 9px;display:inline-block;">' + description + "<span></div>";
}

function getRatioDivisions() {
    var obj = {};
    obj.c8B00FF = { index: 0, min: 0, max: 0 }
    obj.c4B0082 = { index: 1, min: 0, max: 0 }
    obj.c0000FF = { index: 2, min: 0, max: 0 }
    obj.c00FF00 = { index: 3, min: 0, max: 0 }
    obj.cFFFF00 = { index: 4, min: 0, max: 0 }
    obj.cFF7F00 = { index: 5, min: 0, max: 0 }
    obj.cFF0000 = { index: 6, min: 0, max: 0 }
    return obj;
}

function getStyleFromRatio(ratio) {
    var color = "#FF0000"; // red
    if (ratio > 0.09) {
        color = "#8B00FF"; // violet
    } else if (ratio > 0.02) {
        color = "#4B0082"; // indigo
    } else if (ratio > 0.01) {
        color = "#0000FF"; // blue
    } else if (ratio > 0.005) {
        color = "#00FF00"; // green
    } else if (ratio > 0.002) {
        color = "#FFFF00"; // yellow
    } else if (ratio > 0.001) {
        color = "#FF7F00"; // orange
    }
    return color;
}

function loadStackedBarChartOptions(chartOptionsTag, chartTag) {
    var chartOptions = $(chartOptionsTag);
    var url = chartOptions.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    chartOptions.change(function () {
        loadStackedBarChart(chartTag);
    });
    var chartCanvas = $(chartTag);
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroup");
    var statisticDataGrouping = chartCanvas.data("statisticdatagrouping");
    becosoft.ajax(url, {
        data: { statisticType: statisticType, statisticGroups: statisticGroups, statDataGrouping: statisticDataGrouping },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) {
            $(data).each(function () {
                $(document.createElement("option"))
                    .val(this)
                    .text(this)
                    .appendTo(chartOptions);
            });
            $(chartOptionsTag + " option[value='" + $(data).first() + "']").prop("selected", true);
            loadStackedBarChart(chartTag);
        }
    });
}

function loadTableOptions(chartOptionsTag, chartTag) {
    var chartOptions = $(chartOptionsTag);
    var url = chartOptions.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    chartOptions.change(function () {
        loadTable(chartTag);
    });
    const chartCanvas = $(chartTag);
    const statisticType = chartCanvas.data("statistictype");
    const statisticGroups = chartCanvas.data("statisticgroup");
    const statisticDataGrouping = chartCanvas.data("statisticdatagrouping");
    const turnoverType = chartOptions.attr("data-turnovertype");
    becosoft.ajax(url, {
        data: { statisticType: statisticType, statisticGroups: statisticGroups, statDataGrouping: statisticDataGrouping, turnoverType : turnoverType },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) {
            $(data).each(function () {
                $(document.createElement("option"))
                    .val(this)
                    .text(this)
                    .appendTo(chartOptions);
            });
            $(chartOptionsTag + " option[value='" + $(data).first() + "']").prop("selected", true);
            loadTable(chartTag);
        }
    });
}

function loadTable(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var dashboardConfigId = chartTag.split("-")[1];
    //var statisticType = chartCanvas.data("statistictype");
    //var statisticDataGrouping = chartCanvas.data("statisticdatagrouping");
    var statisticGroups = chartCanvas.data("statisticgroup");
    var year = chartCanvas.parent().find("select[id*='chart-options-']").val() || 0;
    becosoft.ajax(url, {
        data: {
            id: dashboardConfigId, year: year, group: statisticGroups
        },
        type: "GET",
        dataType: "json",
        async: true,
        success: function(data) {
            const cont = $(chartTag);
            cont.html(data);
        }
    });
}

function loadStackedBarChart(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var dashboardConfigId = chartTag.split("-")[1];
    //var statisticType = chartCanvas.data("statistictype");
    //var statisticDataGrouping = chartCanvas.data("statisticdatagrouping");
    var statisticGroups = chartCanvas.data("statisticgroup");
    var year = chartCanvas.parent().find("select[id*='chart-options-']").val() || 0;
    becosoft.ajax(url, {
        data: {
            id: dashboardConfigId, year: year, statisticGroups: statisticGroups
        },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) { loadStackedBarChartData(chartTag, data);  }
    });
}

function loadStackedBarChartData(chartTag, chartData) {
    var chartCanvas = $(chartTag);
    var canvas = document.getElementById(chartTag.replace("#", ""));
    if (loadedCharts[chartTag]) {
        loadedCharts[chartTag].destroy();
    }
    var datasetsArray = [];
    var hideYAxis = chartData["HideYAxis"];
    var disableTooltip = chartData["DisableTooltip"];
    var hidePercentage = chartData["HidePercentages"];
    let ignoreMaxValue = chartData["IgnoreMaxValue"];
    $.each(chartData["DataSets"],
        function (key, value) {
            var dataSet = {
                data: value["Data"],
                label: value["Label"],
                backgroundColor: value["Color"],
                borderColor: value["BorderColor"],
                hidden: value["Disabled"],
                type: value["Type"].toLowerCase()
            };
            if (dataSet.borderColor !== null && dataSet.borderColor !== "" && isElPropColor(dataSet.borderColor, "rgb(0,0,0)")) {
                dataSet.borderWidth = 0.5;
            }
            datasetsArray.push(dataSet);
        });
    var tooltips = {
        callbacks: {
            label: function (tooltipItem, data) {
                var allData = data.datasets[tooltipItem.datasetIndex].data;
                var label = data.datasets[tooltipItem.datasetIndex].label;
                var tooltipData = allData[tooltipItem.index];
                var total = 0;
                for (var i in allData) {
                    if (!allData.hasOwnProperty(i)) {
                        continue;
                    }
                    total += allData[i];
                }
                if (hidePercentage) {
                    return label + ": " + tooltipData;
                } else {
                    return label + ": " + Number(tooltipData).formatDecimal(2) + "%";
                }
            }
        }
    }
    if (disableTooltip) {
        tooltips.enabled = false;
    }
    if (hideYAxis === false) {
        chartCanvas.css("margin-top", "-10px");
    }
    var myChart = new Chart(canvas,
    {
        type: "bar",
        data: {
            labels: chartData["Labels"],
            datasets: datasetsArray
        },
        options: {
            maintainAspectRatio: false,
            legend: {
                display: hideYAxis ? false : true,
                onClick: function (evt, item) {
                    var index = item.datasetIndex;
                    var ci = this.chart;
                    var meta = ci.getDatasetMeta(index);

                    // See controller.isDatasetVisible comment
                    meta.hidden = meta.hidden === null? !ci.data.datasets[index].hidden : null;
                    var maxValues = [];
                    for (var dsIndex = 0; dsIndex < ci.data.datasets.length; dsIndex++) {
                        var it = ci.data.datasets[dsIndex];
                        var dsMeta = ci.getDatasetMeta(dsIndex);
                        var isHidden = dsMeta.hidden === null ? it.hidden : dsMeta.hidden;
                        if (isHidden === true) {continue;}
                        for (var i = 0; i < it.data.length; i++) {
                            var existing = maxValues[i];
                            var mValue = it.data[i];
                            if (existing === undefined) {
                                maxValues.push(mValue);
                            } else {
                                maxValues[i] += mValue;
                            }
                        }
                    }
                    if (ignoreMaxValue) {
                        ci.update();
                        return;
                    }
                    var maxValue = maxValues.length > 0 ? maxValues.reduce(function(a, b) {
                        return Math.max(a, b);
                    }) : 0;
                    if (parseInt(maxValue) >= 75) {
                        maxValue = 100;
                    }
                    if (parseInt(maxValue) === 100) {
                        Object.assign(myChart.options.scales.yAxes, [{
                            stacked: true,
                            ticks: {
                                max: 100
                            }
                        }]);
                    } else {
                        Object.assign(myChart.options.scales.yAxes, [{
                            stacked: true
                        }]);
                    }
                    // We hid a dataset ... rerender the chart
                    ci.update();
                }
            },
            scales: {
                xAxes: [{
                    stacked: true,
                    ticks: { autoSkip: chartData["AutoSkipXAxesTicks"] }
                }],
                yAxes: [{
                    stacked: true
                }]
            },
            tooltips: tooltips
        }
    });
    loadedCharts[chartTag] = myChart;
}

function loadBarChartOptions(chartOptionsTag, chartTag) {
    var chartOptions = $(chartOptionsTag);
    var url = chartOptions.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    chartOptions.change(function () {
        loadBarChartWithOptions(chartTag);
    });
    var chartCanvas = $(chartTag);
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroup");
    var statisticDataGrouping = chartCanvas.data("statisticdatagrouping");
    becosoft.ajax(url, {
        data: { statisticType: statisticType, statisticGroups: statisticGroups, statDataGrouping: statisticDataGrouping },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) {
            $(data).each(function () {
                $(document.createElement("option"))
                    .val(this)
                    .text(this)
                    .appendTo(chartOptions);
            });
            $(chartOptionsTag + " option[value='" + $(data).first() + "']").prop("selected", true);
            loadBarChartWithOptions(chartTag);
        }
    });
}

function loadBarChartWithOptions(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var dashboardConfigId = chartTag.split("-")[1];
    var statisticGroups = chartCanvas.data("statisticgroup");
    var year = chartCanvas.parent().find("select[id*='chart-options-']").val();
    becosoft.ajax(url, {
        data: {
            id: dashboardConfigId, year: year, statisticGroups: statisticGroups
        },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) { loadBarChartData(chartTag, chartData); }
    });
}

function loadBarChartData(chartTag, chartData) {
    var canvas = document.getElementById(chartTag.replace("#", ""));
    var chartCanvas = $(chartTag);
    if (loadedCharts[chartTag]) {
        loadedCharts[chartTag].destroy();
    }
    var datasetsArray = [];
    var hideYAxis = chartData["HideYAxis"];
    $.each(chartData["DataSets"],
        function (key, value) {
            datasetsArray.push({
                data: value["Data"],
                label: value["Label"],
                backgroundColor: value["Color"],
                hidden: value["Disabled"]
            });
        });
    if (hideYAxis === false) {
        chartCanvas.css("margin-top", "-10px");
    }
    var myChart = new Chart(canvas,
        {
            type: "bar",
            data: {
                labels: chartData["Labels"],
                datasets: datasetsArray
            },
            options: {
                maintainAspectRatio: false,
                legend: {
                    display: !hideYAxis
                },
                scales: {
                    yAxes: [
                        {
                            ticks: {
                                beginAtZero: true
                            }
                        }
                    ],
                    xAxes: [
                        {
                            ticks: {
                                autoSkip: false,
                                beginAtZero: true
                            }
                        }
                    ]
                }
            }
        });
    loadedCharts[chartTag] = myChart;
}

function loadBarChart(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var statisticType = chartCanvas.data("statistictype");
    var configId = chartCanvas.attr("id").split("-")[1];
    var context = document.getElementById(chartTag.replace("#", "")).getContext("2d");
    becosoft.ajax(url, {
        data: { id: configId, statisticType: statisticType },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) {
            if (loadedCharts[chartTag]) {
                loadedCharts[chartTag].destroy();
            }
            var datasetsArray = [];
            var hasDataLabels = false;
            $.each(chartData["DataSets"],
                function (key, value) {
                    const lbl = value["Label"];
                    datasetsArray.push({
                        data: value["Data"],
                        label: lbl,
                        backgroundColor: value["Color"],
                        hidden: value["Disabled"]
                    });
                    if (lbl !== undefined && lbl != null && lbl !== "") {
                        hasDataLabels = true;
                    }
                });
            var myChart = new Chart(context,
            {
                type: "bar",
                data: {
                    labels: chartData["Labels"],
                    datasets: datasetsArray
                },
                options: {
                    maintainAspectRatio: false,
                    legend: {
                        display: hasDataLabels
                    },
                    scales: {
                        yAxes: [
                            {
                                ticks: {
                                    beginAtZero: true
                                }
                            }
                        ]
                    }
                }
            });
            loadedCharts[chartTag] = myChart;
        }
    });
}

function loadLineBarChart(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroup");
    var dashboardConfigId = chartTag.split("-")[1];
    var data = { id: dashboardConfigId, statisticType: statisticType };
    if (statisticGroups.length) {
        data.statisticGroups = statisticGroups;
    }
    var context = document.getElementById(chartTag.replace("#", "")).getContext("2d");
    becosoft.ajax(url, {
        data: data,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) {
            if (loadedCharts[chartTag]) {
                loadedCharts[chartTag].destroy();
            }
            var datasetsArray = [];
            var hasLine = false;
            var hasBar = false;
            var hideYAxis = chartData["HideYAxis"];
            var startYAtZero = false;
            var lineMax = 0.0;
            var barMax = 0.0;
            $.each(chartData["DataSets"],
                function (key, value) {
                    var lowerType = value["Type"].toLowerCase();
                    if (lowerType === "bar") {
                        hasBar = true;
                        if (value["MaxValue"] !== null) {
                            barMax = parseFloat(value["MaxValue"]);
                        }
                    }
                    if (lowerType === "line") {
                        hasLine = true;
                        if (value["BeginAtZero"] === true) {
                            startYAtZero = true;
                        }
                        if (value["MaxValue"] !== null) {
                            lineMax = parseFloat(value["MaxValue"]);
                        }
                    }
                });
            $.each(chartData["DataSets"],
                function (key, value) {
                    var lowerType = value["Type"].toLowerCase();
                    datasetsArray.push({
                        data: value["Data"],
                        backgroundColor: value["Color"],
                        type: lowerType,
                        label: value["Label"],
                        fill: (lowerType !== "line" && hasBar === true) || (!hasBar && lowerType === "line"),
                        yAxisID: lowerType,
                        borderColor: value["BorderColor"] ?? value["Color"],
                        hidden: value["Disabled"]
                    });
                });
            console.log(datasetsArray);
            var yAxes = [];
            if (hasLine) {
                var lineAxisObj = {
                    ticks: {
                        beginAtZero: startYAtZero
                    },
                    position: !hasLine ? "left" : "right",
                    id: "line"
                };
                if (Math.abs(lineMax - 0.0) > Number.EPSILON) {
                    lineAxisObj.ticks.max = lineMax;
                }
                yAxes.push(lineAxisObj);
            }
            if (hasBar) {
                var barAxisObj = {
                    ticks: {
                        beginAtZero: true
                    },
                    position: "left",
                    id: "bar"
                };
                if (Math.abs(barMax - 0.0) > Number.EPSILON) {
                    barAxisObj.ticks.max = barMax;
                }
                yAxes.push(barAxisObj);
            }
            //console.log(yAxes);
            var myChart = new Chart(context,
            {
                type: "bar",
                data: {
                    labels: chartData["Labels"],
                    datasets: datasetsArray
                },
                options: {
                    maintainAspectRatio: false,
                    legend: {
                        display: hideYAxis ? false : true
                    },
                    scales: {
                        yAxes: yAxes
                    }
                }
            });
            loadedCharts[chartTag] = myChart;
        }
    });
}

function loadPieChart(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroup = chartCanvas.data("statisticgroup");
    var dashboardConfigId = chartTag.split("-")[1];
    var parameterdata = { id: dashboardConfigId, statisticType: statisticType, statisticGroup: statisticGroup };
    loadPieChartFromUrl(chartTag, url, parameterdata);
}

function loadPieChartFromUrl(chartTag, url, parameters) {
    becosoft.ajax(url, {
        data: parameters,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) {
            if (loadedCharts[chartTag]) {
                loadedCharts[chartTag].destroy();
            }
            var hidePercentage = chartData["HidePercentages"];
            var hideYAxis = chartData["HideYAxis"];
            var datasetsArray = [];
            $.each(chartData["DataSets"],
                function (key, value) {
                    datasetsArray.push({
                        data: value["Data"],
                        backgroundColor: value["Colors"]
                    });
                });
            var context = document.getElementById(chartTag.replace("#", "")).getContext("2d");
            var myChart = new Chart(context,
            {
                type: "pie",
                data: {
                    labels: chartData["Labels"],
                    datasets: datasetsArray
                },
                options: {
                    maintainAspectRatio: false,
                    legend: {
                        display: hideYAxis ? false : true,
                        labels: {
                            fontSize: 12,
                            boxWidth: 20
                        }
                    },
                    tooltips: {
                        callbacks: {
                            label: function (tooltipItem, data) {
                                var allData = data.datasets[tooltipItem.datasetIndex].data;
                                var tooltipLabel = data.labels[tooltipItem.index];
                                var tooltipData = allData[tooltipItem.index];
                                var total = 0;
                                for (var i in allData) {
                                    if (!allData.hasOwnProperty(i)) {
                                        continue;
                                    }
                                    total += allData[i];
                                }
                                if (hidePercentage) {
                                    return tooltipLabel + ": " + tooltipData;
                                } else {
                                    var tooltipPercentage = Math.round((tooltipData / total) * 100);
                                    return tooltipLabel + ": " + tooltipData + " (" + tooltipPercentage + "%)";
                                }
                            }
                        }
                    }
                }
            });
            loadedCharts[chartTag] = myChart;
        }
    });
}

function loadStackedBarChartFromUrl(chartTag, url, parameters) {
    becosoft.ajax(url, {
        data: parameters,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) { loadStackedBarChartData(chartTag, chartData); }
    });
}

function loadBarChartFromUrl(chartTag, url, parameters) {
    becosoft.ajax(url, {
        data: parameters,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (chartData) { loadBarChartData(chartTag, chartData); }
    });
}

function loadCustom(id) {
    var customContainer = $(id);
    let url = customContainer.data("url");
    let dashboardConfigId = id.split("-")[1];
    let parameters = {
        id: dashboardConfigId,
        group: customContainer.data("statisticgroups")
    }
    becosoft.ajax(url, {
        data: parameters,
        type: "POST",
        dataType: "json",
        async: true,
        success: function(customView) {
            let childDiv = customContainer.find("div:first");
            childDiv.height(customContainer.parents("div.card-body").height());
            childDiv.addClass("w-100");
            childDiv.css({ "overflow-x" :"hidden","overflow-y":"scroll" });
            childDiv.html(customView);
            let headers = childDiv.find("th");
            headers.on('click', function(e) {
                console.log($(this));
                sortTable(e, {stickyRowAttr:"data-total-row"});
            });
            headers.hover(function() {
                $(this).css('cursor', 'pointer');
            });
            childDiv.find('[data-toggle="tooltip"]').tooltip();

        }
    });
}

function loadChartOptions(chartOptions, chartTag) {
    var select = $(chartOptions);
    var url = select.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var supplierChartTag = '#supplier-' + chartOptions.replace("#", "");
    var supplierChartElem = $(supplierChartTag);
    if (supplierChartElem.length > 0) {
        select.change(function() {
            loadChartSuppliers(chartTag, chartOptions, supplierChartTag);
        });
    } else {
        select.change(function () {
            loadChartData(chartTag);
        });
    }
    var chartCanvas = $(chartTag);
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroups");
    var statisticDataGrouping = parseInt(chartCanvas.data("statisticdatagrouping"));
    becosoft.ajax(url, {
        data: { statisticType: statisticType, statisticGroups: statisticGroups, statDataGrouping: statisticDataGrouping },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) {
            $(data).each(function () {
                $(document.createElement("option"))
                    .val(this)
                    .text(this)
                    .appendTo(select);
            });
            $(chartOptions + " option[value='" + $(data).first() + "']").prop("selected", true);
            if (supplierChartElem.length > 0) {
                loadChartSuppliers(chartTag, chartOptions, supplierChartTag);
            } else {
                loadChartData(chartTag);
            }
        },
        failure: function (data) {
            console.log(data);
        }
    });
}

function loadChartSuppliers(chartTag, chartOptions, supplierChartOptions) {
    var supplierSelection = $(supplierChartOptions);
    var url = supplierSelection.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    supplierSelection.change(function () {
        loadChartData(chartTag);
    });
    var chartCanvas = $(chartTag);
    var yearSelection = $(chartOptions);
    var year = yearSelection.val();
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroups");
    var statisticDataGrouping = parseInt(chartCanvas.data("statisticdatagrouping"));
    becosoft.ajax(url, {
        data: { statisticType: statisticType, statisticGroups: statisticGroups, statDataGrouping: statisticDataGrouping, year: year },
        type: "GET",
        dataType: "json",
        async: true,
        success: function (data) {
            $(data).each(function () {
                $(document.createElement("option"))
                    .val(this)
                    .text(this)
                    .appendTo(supplierSelection);
            });
            $(supplierChartOptions + " option[value='" + $(data).first() + "']").prop("selected", true);
            loadChartData(chartTag);
        },
        failure: function (data) {
            console.log(data);
        }
    });
}

function loadChartData(chartTag) {
    var chartCanvas = $(chartTag);
    var url = chartCanvas.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    var chartParent = chartCanvas.parent();
    var newHeight = chartParent.height() - chartParent.find("select").first().height();
    var statisticType = chartCanvas.data("statistictype");
    var statisticGroups = chartCanvas.data("statisticgroups");
    var dashboardConfigId = chartTag.split("-")[1];
    var year = chartCanvas.parent().find("select[id*='chart-options-']").val();
    var filter = chartCanvas.parent().find("select[id*='supplier-chart-options-']").val();
    var context = document.getElementById(chartTag.replace("#", "")).getContext("2d");
    becosoft.ajax(url, {
        data: { Id: dashboardConfigId, StatisticType: statisticType, StatisticGroups: statisticGroups, Year: year, FilterValue: filter },
        type: "POST",
        dataType: "json",
        async: true,
        success: function (chartData) {
            if (loadedCharts[chartTag]) {
                loadedCharts[chartTag].destroy();
            }
            var datasetsArray = [];
            var hideYAxis = chartData["HideYAxis"];
            var disableTooltip = chartData["DisableTooltip"];
            $.each(chartData["DataSets"],
                function (key, value) {
                    var dataSet = {
                        label: value["Label"],
                        data: value["Data"],
                        backgroundColor: value["Color"],
                        type: value["Type"].toLowerCase(),
                        borderColor: value["BorderColor"],
                        hidden: value["Disabled"]
                    };
                    if (dataSet.borderColor !== null && dataSet.borderColor !== "" && isElPropColor(dataSet.borderColor, "rgb(0,0,0)")) {
                        dataSet.borderWidth = 0.5;
                    }
                    datasetsArray.push(dataSet);
                });
            var type = datasetsArray[0] != null ? datasetsArray[0].type : "line";
            var scales = {
                yAxes: [
                    {
                        ticks: {
                            callback: function (label, index, labels) {
                                return Number(label).formatDecimal(2);
                            }
                        }
                    }
                ]
            };
            if (hideYAxis) {
                scales.yAxes[0].display = false;
            }
            if (datasetsArray.length > 1) {
                var barCount = 0;
                $.each(datasetsArray, function (key, value) {
                    if (value.type === "bar") {
                        barCount += 1;
                    }
                });
                if (barCount > 1) {
                    type = "bar";
                    scales = {
                        yAxes: [{
                            ticks: {
                                callback: function (label, index, labels) {
                                    return Number(label).formatDecimal(2);
                                }
                            },
                            stacked: true
                        }],
                        xAxes: [{
                            stacked: true,
                            ticks: {autoSkip: chartData["AutoSkipXAxesTicks"]}
                        }]
                    }
                }
                if (datasetsArray.length > 3) {
                    $.each(datasetsArray, function (key, value) {
                        if (value.type === "line") {
                            value.fill = false;
                        }
                    });
                }
            }
            var tooltips = {
                callbacks: {
                    label: function (tooltipItem, data) {
                        return Number(tooltipItem.yLabel).formatDecimal(2);
                    }
                }
            }
            if (disableTooltip) {
                tooltips.enabled = false;
            }
            var myChart = new Chart(context,
            {
                type: type,
                data: {
                    labels: chartData["Labels"],
                    datasets: datasetsArray
                },
                options: {
                    tooltips: tooltips,
                    scales: scales,
                    maintainAspectRatio: false
                }
            });
            loadedCharts[chartTag] = myChart;
            chartCanvas.height(newHeight);
        }
    });
}

function loadPeriodData(aTag, liTag) {
    if (aTag.data("defaultturnovertype") !== "" && aTag.attr("id").split("-")[2] === "0") {
        var defaultTurnovertype = parseInt(aTag.data("defaultturnovertype"));
        if (defaultTurnovertype === 8) {
            defaultTurnovertype = 0;
        } else if (defaultTurnovertype > 4) {
            defaultTurnovertype -= 3;
        }
        aTag.data("defaultturnovertype", "");
        if (defaultTurnovertype !== 0 && !isNaN(defaultTurnovertype)) {
            var configId = aTag.attr("id").split("-")[1];
            aTag = $("#period-" + configId + "-" + defaultTurnovertype);
        }
    }
    var href = aTag.attr("href");
    var turnoverType = "0";
    var groupUlId = aTag.attr("id").replace("period-", "period-group-pill-");
    var groupUl = $("#" + groupUlId);
    var groupLevel = 0;
    var url = aTag.data("url");
    if (url == null || url.length === 0) {
        return;
    }
    if (groupUl !== undefined && groupUl !== null && groupUl.length !== 0) {
        var firstGroupLichild = groupUl.children("li.active").first();
        if (firstGroupLichild === undefined || firstGroupLichild === null || firstGroupLichild.length === 0) {
            firstGroupLichild = groupUl.children("li:first-child").first();
            firstGroupLichild.addClass("active");
            firstGroupLichild.find("a").first().addClass("active");
        }
        groupLevel = firstGroupLichild.data("grouplevel");
    }
    if (liTag === undefined || liTag === null) {
        var ulid = "#" + aTag.attr("id").replace("period-", "period-pill-");
        var ultag = $(ulid);
        var firstlichild = ultag.children("li.active").first();
        if (firstlichild === undefined || firstlichild === null || firstlichild.length === 0) {
            firstlichild = ultag.children("li:first-child").first();
            firstlichild.addClass("active");
            firstlichild.find("a").first().addClass("active");
        }
        turnoverType = firstlichild.data("turnovertype");
    } else {
        turnoverType = liTag.data("turnovertype");
        liTag.parent().children().each(function () {
            $(this).removeClass("active");
            $(this).find("a").first().removeClass("active");
        });
        liTag.addClass("active");
        liTag.find("a").first().addClass("active");
    }
    var statisticGroup = aTag.data("statisticgroup");
    var statisticType = Number(aTag.data("statistictype"));
    var isSalesGroupProportion = aTag.data("issalesgroupproportion") === "True";
    if (isSalesGroupProportion) {
        var div = $(href);
        var contentData = div.find('div[id*="period-content-data-"]');
        var canvasTag = contentData.find('div > canvas[data-isprevious=0]').attr("id");
        var parameterdata = { id: aTag.attr("id").split("-")[1], year: 0, statisticGroups: statisticGroup, turnoverType: turnoverType};
        loadBarChartFromUrl(canvasTag, url, parameterdata);
        $(aTag).tab("show");
        return;
    }
    var isDashboardStatistics = aTag.data("isdashboardstatistics") === "True";
    var isBrandStat = aTag.data("isbrandstat") === "True";
    var isGroupStat = aTag.data("isgroupstat") === "True";
    var isGroupSupplierStatistics = aTag.data("isgroupsupplierstatistics") === "True";
    var isSalesEvolution = aTag.data("issalesevolution") === "True";
    var data = {};
    if (isDashboardStatistics) {
        data = { turnoverType: turnoverType, group: statisticGroup };
    } else if (isGroupSupplierStatistics) {
        if (isGroupStat) {
            data = {
                turnoverType: turnoverType,
                group: statisticGroup,
                statisticType: statisticType,
                level: groupLevel
            };
        } else {
            data = { turnoverType: turnoverType, group: statisticGroup, statisticType: statisticType };
        }
    } else if (isSalesEvolution) {
        data = { turnoverType: turnoverType };
    }
    data.id = aTag.attr("id").split("-")[1];
    becosoft.ajax(url, {
        data: data,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (resultData) {
            if (isDashboardStatistics || isSalesEvolution) {
                updatePeriodData(href, resultData, aTag);
            } else if (isGroupSupplierStatistics) {
                updatePeriodDataList(href, resultData, aTag, isGroupStat);
            }
        }
    });
}

function updatePeriodDataList(containerDiv, data, clickedParent, isGroupStat) {
    var div = $(containerDiv);
    var contentData = div.find('div[id*="period-content-data-"]');
    contentData.html(data);
    $(clickedParent).tab("show");
    var panelBody = contentData.closest(".panel-body");
    var panelBodyHeight = panelBody.height();
    var panelBodyUl = panelBody.children("ul").first();
    var newHeight = panelBodyUl.outerHeight();
    contentData.parent().children("ul").each(function () {
        newHeight += $(this).height();
    });
    var t = contentData.children("table").first();
    contentData.height(panelBodyHeight - newHeight);
    t.find(".pointer-popover").each(function () {
        $(this).on("click", function (eventData) {
            loadPopoverData(eventData, clickedParent);
        });
    });
    if (isGroupStat) {
        t.find(".next-group-level").each(function () {
            $(this).on("click", function (eventData) {
                loadNextGroupData(eventData, clickedParent);
            });
        });
    }
}

function updatePeriodData(containerDiv, data, clickedParent) {
    const div = $(containerDiv);
    const parentDiv = div.closest('div.dashboard-stat');
    const table = parentDiv.find('table[data-current=true]');
    const tableElemDataStr = table.data('elements-visibility');
    parentDiv.find('table[data-customcollapsible=""][data-current]').removeAttr('data-current');
    const elementVisibilityData = tableElemDataStr === undefined ? undefined : JSON.parse(tableElemDataStr);
    const contentData = div.find('div[id*="period-content-data-"]');
    contentData.html(data);
    $(clickedParent).tab("show");
    contentData.find("table[data-customcollapsible='']").attr('data-current', true);

    if (elementVisibilityData !== undefined) {
        loadElementsVisibility(contentData.find("table[data-customcollapsible='']"), elementVisibilityData);
    }
    handleCollapsibleTableRowBackground(contentData.find("table[data-customcollapsible='']"));
}

function loadPopoverData(eventData, clickedParent) {
    var popoverelem = $(eventData.currentTarget);
    var tr = popoverelem.closest("tr");
    var didClose = false;
    var popover = $(".popover.show");
    if (popover.length > 0) {
        if (tr.attr("data-popover-enabled")) {
            didClose = true;
            tr.attr("data-popover-enabled", "");
        }
        var enabled = $("tr[data-popover-enabled=true]");
        enabled.attr("data-popover-enabled", "");
        popover.popover("hide");
        popover.popover("dispose");
    }
    if (didClose === true) {
        return;
    }
    var data = parseDataObject(tr);
    var url = clickedParent.data("popoverurl");
    becosoft.ajax(url, {
        data: data,
        type: "GET",
        dataType: "json",
        async: true,
        success: function (resultData) {
            var span = popoverelem.parent().find(".next-group-level").first();
            if (span == null || span.length === 0) {
                span = $(popoverelem.parent().find("span")[1]);
            }
            var options = { content: resultData, trigger: "focus", placement: "auto", title: span.text(), html: true };
            popoverelem.popover("hide");
            popoverelem.popover("dispose");
            popoverelem.popover(options);
            tr.attr("data-popover-enabled", true);
            popoverelem.on("shown.bs.popover",
                function() {
                    var span = $(this);
                    var popoverDiv = span.next(".popover");
                    popoverDiv.find(".next-group-level").each(function () {
                        var span = $(this);
                        span.on("click", function (evData) {
                            loadNextGroupData(evData, clickedParent);
                        });
                    });
                });
            popoverelem.popover("show");
            //console.log(resultData);
        }
    });
}

function loadNextGroupData(eventData, clickedParent) {
    var popoverelem = $(eventData.currentTarget);
    var tr = popoverelem.closest("tr");
    var nextTr = tr.next();
    if (nextTr != null && nextTr.hasClass("next-level-result")) {
        nextTr.remove();
        return;
    }
    var data = parseDataObject(tr);
    var url = clickedParent.data("nextlevel");
    becosoft.ajax(url, {
        data: data,
        type: "GET",
        dataType: "text",
        async: true,
        success: function (resultData) {
            var newTr = $("<tr class='next-level-result' style='font-size:9px;'><td colspan='6'>" + resultData + "</td></tr>");
            tr.after(newTr);
            newTr.find(".next-group-level").each(function () {
                var span = $(this);
                span.on("click", function (evData) {
                    loadNextGroupData(evData, clickedParent);
                });
            });
            newTr.find(".pointer-popover").each(function () {
                var span = $(this);
                span.on("click", function (evData) {
                    loadPopoverData(evData, clickedParent);
                });
            });
            //console.log(resultData);
        }
    });
}

function parseDataObject(elem) {
    var rawData = elem.data("object");
    rawData = rebuildHtmlCharacters(rawData);
    var data = JSON.parse(rawData);
    return data;
}

function rebuildHtmlCharacters(rawData) {
    rawData = rawData.replaceAll("&amp;", "&");
    rawData = rawData.replaceAll("&lt;", "<");
    rawData = rawData.replaceAll("&gt;", ">");
    rawData = rawData.replaceAll("&#x2F;", "/");
    rawData = rawData.replaceAll("&#x60;", "`");
    rawData = rawData.replaceAll("&#x3D;", "=");
    rawData = rawData.replaceAll("'", "\"");
    return rawData;
}
