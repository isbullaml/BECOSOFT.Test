/*global google */
/*global charts */
/*global setOnLoadCallback */
/*global visualization */
/*global arrayToDataTable */
/*global setValue */
/*global getServerInfoUrl */
/*global getSolutionUrl */
/*global Gauge */

// info: https://developers.google.com/chart/interactive/docs/gallery/gauge

$(function() {
    window.google.charts.load("current", { 'packages': ["gauge"] });
    window.google.charts.setOnLoadCallback(initCharts);

    if (document.addEventListener) {
        window.addEventListener("resize", resizeChart);
    } else if (document.attachEvent) {
        window.attachEvent("onresize", resizeChart);
    } else {
        window.resize = resizeChart;
    }

    var sqlData = null;
    var webData = null;
    var options = null;
    var sqlChart = null;
    var webChart = null;

    // Update values when a selectList changes
    $("select").change(function () {
        var sql = parseInt($("#sqlDD option:selected").val());
        var web = parseInt($("#webDD option:selected").val());
        var sol = parseInt($("#solDD option:selected").val());
        var btnConfirm = $("#btnConfirm");
        if (sql === 0 && web === 0 || sol === 0) btnConfirm.prop("disabled", true);
        else btnConfirm.prop("disabled", false);
    });

    function initCharts() {
        // Initialize SQL-data
        sqlData = window.google.visualization.arrayToDataTable([
            ["Label", "Value"],
            ["Memory", 100],
            ["CPU", 100],
            ["Disk", 100]
        ]);

        // Initialize web-data
        webData = window.google.visualization.arrayToDataTable([
            ["Label", "Value"],
            ["Memory", 100],
            ["CPU", 100],
            ["Disk", 100]
        ]);

        // Set options for the gauges
        options = {
            redColor: "#f05717",
            yellowColor: "#efb718",
            greenColor: "#26a66c",
            redFrom: 90,
            redTo: 100,
            yellowFrom: 75,
            yellowTo: 90,
            greenFrom: 0,
            greenTo: 75,
            minorTicks: 5,
            animation: {
                duration: 500,
                easing: "linear"
            }
        };

        // Set a formatter to display '%' after the number on the gauge
        var formatter = new window.google.visualization.NumberFormat(
            { suffix: "%", pattern: "#" }
        );

        // Initialize SQL-chart
        sqlChart = new window.google.visualization.Gauge(document.getElementById("sqlChart"));
        sqlChart.draw(sqlData, options);
        setChart(true, 0, 0, 0);

        // Initialize web-chart
        webChart = new window.google.visualization.Gauge(document.getElementById("webChart"));
        webChart.draw(webData, options);
        setChart(false, 0, 0, 0);

        // Redraw the chart
        resizeChart();

        // Sets the values on the right chart
        function setChart(sql, ram, cpu, disk) {
            if (sql) {
                sqlData.setValue(0, 1, ram);
                sqlData.setValue(1, 1, cpu);
                sqlData.setValue(2, 1, disk);
                formatter.format(sqlData, 1);
                sqlChart.draw(sqlData, options);
            } else {
                webData.setValue(0, 1, ram);
                webData.setValue(1, 1, cpu);
                webData.setValue(2, 1, disk);
                formatter.format(webData, 1);
                webChart.draw(webData, options);
            }
        }

        // Event handler for changing the SQL-dropdown
        $("#sqlDD").change(function() {
            var value = $(this).val();
            if (value === "0") {
                setChart(true, 0, 0, 0);
            } else {
                becosoft.ajax(window.getServerInfoUrl + "/" + value, { type: "GET" }, function (data) {
                    setChart(true, data.RamPercentage, data.CpuPercentage, data.DiskPercentage);
                });
            }
        });

        // Event handler for changing the web-dropdown
        $("#webDD").change(function() {
            var value = $(this).val();
            if (value === "0") {
                setChart(false, 0, 0, 0);
            } else {
                becosoft.ajax(window.getServerInfoUrl + "/" + value, { type: "GET" }, function (data) {
                    setChart(false, data.RamPercentage, data.CpuPercentage, data.DiskPercentage);
                });
            }
        });

        // Event handler for changing the solution-dropdown
        $("#solDD").change(function() {
            var value = $(this).val();
            if (value !== "0") {
                becosoft.ajax(window.getSolutionUrl + "/" + value, { type: "GET" }, function (data) {
                    $("#initPrice").text(data.InitialCost);
                    $("#monthPrice").text(data.PricePerMonth);
                });
            }
        });
    }

    // Resize the chart
    function resizeChart() {
        console.log("Resizing");
        sqlChart.draw(sqlData, options);
        webChart.draw(webData, options);
    }
});