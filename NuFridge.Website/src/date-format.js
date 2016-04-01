import moment from 'moment';

export class DateFormatValueConverter {
    toView(value) {
        if (value) {
            return moment(value).format('MMMM Do YYYY HH:mm:ss');
        }
        return "";
    }
}