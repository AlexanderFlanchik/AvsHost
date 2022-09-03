const moment = require('moment');

export const formatDate = (d: Date) => d ? moment(d).format('MM/DD/YYYY hh:mm:ss A') : '-';