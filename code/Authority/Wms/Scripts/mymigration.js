﻿function begin(url) {
    var myDate = new Date();
    var year = myDate.getFullYear() - 1;
    var month = myDate.getMonth() + 1;
    var date = myDate.getDate();
    var datetime = year + "-" + month + "-" + date;
    $.messager.confirm('提示', '确认要迁移“一年前”的数据吗？', function (r) {
        if (r) {
            $.ajaxSender.send(url, { 'datetime': datetime });
        }
    });
}