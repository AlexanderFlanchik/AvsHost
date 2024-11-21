import moment from 'moment';

export const formatDate = (d: Date | null | undefined) => d ? moment(d).format('MM/DD/YYYY hh:mm:ss A') : '-';