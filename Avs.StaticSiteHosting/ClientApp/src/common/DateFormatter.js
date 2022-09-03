"use strict";
exports.__esModule = true;
exports.formatDate = void 0;
var moment = require('moment');
var formatDate = function (d) { return d ? moment(d).format('MM/DD/YYYY hh:mm:ss A') : '-'; };
exports.formatDate = formatDate;
