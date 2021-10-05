﻿(function ($) {
    window.chartOption = {
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                title: {
                    display: true,
                    text: null
                }
            },
            tooltips: {
                mode: 'index',
                intersect: false
            },
            hover: {
                mode: 'nearest',
                intersect: true
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: false,
                        text: null
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: false,
                        text: null
                    }
                }
            }
        }
    };

    var skipped = (ctx, value) => ctx.p0.skip || ctx.p1.skip ? value : undefined;
    var down = (ctx, value) => ctx.p0.parsed.y > ctx.p1.parsed.y ? value : undefined;

    var genericOptions = {
        fill: false,
        interaction: {
            intersect: false
        },
        radius: 0
    };

    $.extend({
        getChartOption: function (option) {
            var colors = [];
            window.chartColors = option.options.colors;
            for (var name in option.options.colors) colors.push(name);

            var config = {};
            var colorFunc = null;
            if (option.type === 'line') {
                if ($.isArray(option.data)) {
                    $.each(option.data, function (i, ele) {
                        $.each(ele.data, function (j, el) {
                            if (el === null) {
                                option.data[i].data[j] = NaN;
                                option.data[i].segment = {
                                    borderColor: ctx => skipped(ctx, 'rgb(0,0,0,0.2)'),
                                    borderDash: ctx => skipped(ctx, [6, 6])
                                };
                            }
                        });
                    });
                    option.options = $.extend(true, option.options, genericOptions);
                }
                config = $.extend(true, {}, chartOption);
                colorFunc = function (data) {
                    var color = chartColors[colors.shift()]
                    $.extend(data, {
                        backgroundColor: color,
                        borderColor: color
                    });
                }
            }
            else if (option.type === 'bar') {
                config = $.extend(true, {}, chartOption);
                colorFunc = function (data) {
                    var color = chartColors[colors.shift()]
                    $.extend(data, {
                        backgroundColor: Chart.helpers.color(color).alpha(0.5).rgbString(),
                        borderColor: color,
                        borderWidth: 1
                    });
                }
            }
            else if (option.type === 'pie' || option.type === 'doughnut') {
                config = $.extend(true, {}, chartOption, {
                    options: {
                        scales: {
                            x: {
                                display: false
                            },
                            y: {
                                display: false
                            }
                        }
                    }
                });
                colorFunc = function (data) {
                    $.extend(data, {
                        backgroundColor: colors.slice(0, data.data.length).map(function (name) {
                            return chartColors[name];
                        })
                    });
                }

                if (option.type === 'doughnut') {
                    $.extend(config.options, {
                        cutoutPercentage: 50,
                        animation: {
                            animateScale: true,
                            animateRotate: true
                        }
                    });
                }
            }
            else if (option.type === 'bubble') {
                config = $.extend(true, {}, chartOption, {
                    data: {
                        animation: {
                            duration: 10000
                        },
                    },
                    options: {
                        tooltips: {
                            mode: 'point'
                        }
                    }
                });
                colorFunc = function (data) {
                    var color = chartColors[colors.shift()]
                    $.extend(data, {
                        backgroundColor: Chart.helpers.color(color).alpha(0.5).rgbString(),
                        borderWidth: 1,
                        borderColor: color
                    });
                }
            }

            $.each(option.data, function () {
                colorFunc(this);
            });

            return $.extend(true, config, {
                type: option.type,
                data: {
                    labels: option.labels,
                    datasets: option.data
                },
                options: {
                    responsive: option.options.responsive,
                    maintainAspectRatio: option.options.maintainAspectRatio,
                    plugins: {
                        title: {
                            display: option.options.title != null,
                            text: option.options.title
                        }
                    },
                    scales: {
                        x: {
                            title: {
                                display: option.options.x.title != null,
                                text: option.options.x.title
                            },
                            stacked: option.options.x.stacked
                        },
                        y: {
                            title: {
                                display: option.options.y.title != null,
                                text: option.options.y.title
                            },
                            stacked: option.options.x.stacked
                        }
                    }
                }
            });
        },
        updateChart: function (config, option) {
            if (option.updateMethod === "addDataset") {
                config.data.datasets.push(option.data.datasets.pop());
            }
            else if (option.updateMethod === "removeDataset") {
                config.data.datasets.pop();
            }
            else if (option.updateMethod === "addData") {
                if (config.data.datasets.length > 0) {
                    config.data.labels.push(option.data.labels.pop());
                    config.data.datasets.forEach(function (dataset, index) {
                        dataset.data.push(option.data.datasets[index].data.pop());
                        if (option.type === 'pie' || option.type === 'doughnut') {
                            dataset.backgroundColor.push(option.data.datasets[index].backgroundColor.pop());
                        }
                    });
                }
            }
            else if (option.updateMethod === "removeData") {
                config.data.labels.pop(); // remove the label first

                config.data.datasets.forEach(function (dataset) {
                    dataset.data.pop();
                    if (option.type === 'pie' || option.type === 'doughnut') {
                        dataset.backgroundColor.pop();
                    }
                });
            }
            else if (option.updateMethod === "setAngle") {
                if (option.type === 'doughnut') {
                    if (option.angle === 360) {
                        config.options.circumference = 360;
                        config.options.rotation = -360;
                    }
                    else {
                        config.options.circumference = 180;
                        config.options.rotation = -90;
                    }
                }
            }
            else {
                config.data.datasets.forEach((dataset, index) => {
                    dataset.data = option.data.datasets[index].data;
                });
            }
        },
        bb_chart: function (el, obj, method, option, updateMethod, type, angle) {
            var $el = $(el);
            option.type = type;
            var chart = $el.data('chart');
            if (!chart) {
                var op = $.getChartOption(option);
                $el.data('chart', chart = new Chart(el.getElementsByTagName('canvas'), op));
                $el.removeClass('is-loading').trigger('chart.afterInit');
                obj.invokeMethodAsync(method);
            }
            else {
                var op = $.getChartOption(option);
                op.angle = angle;
                op.updateMethod = updateMethod;
                $.updateChart(chart.config, op);
                chart.update();
            }
        }
    });
})(jQuery);
