import {Enum} from 'enum';

export const Claims = new Enum({
    SystemAdministrator: {value: 'SystemAdministrator'},
    CanInsertFeed: {value: 'CanInsertFeed'},
    CanUpdateFeed: {value: 'CanUpdateFeed'},
    CanViewUsers: {value: 'CanViewUsers'},
    CanDeleteFeed: {value: 'CanDeleteFeed'},
    CanViewFeeds: {value: 'CanViewFeeds'},
    CanViewDashboard: {value: 'CanViewDashboard'},
    CanViewFeedHistory: {value: 'CanViewFeedHistory'},
    CanUploadPackages: {value: 'CanUploadPackages'},
    CanReindexPackages: {value: 'CanReindexPackages'},
    CanExecuteRetentionPolicy: {value: 'CanExecuteRetentionPolicy'},
    CanChangePackageDirectory: {value: 'CanChangePackageDirectory'},
    CanDeletePackages: {value: 'CanDeletePackages'},
    CanDownloadPackages: {value: 'CanDownloadPackages'},
    CanViewPackages: {value: 'CanViewPackages'},
    CanUpdateFeedGroup: {value: 'CanUpdateFeedGroup'},
    CanInsertFeedGroup: {value: 'CanInsertFeedGroup'},
    CanUpdateUsers: {value: 'CanUpdateUsers'},
});