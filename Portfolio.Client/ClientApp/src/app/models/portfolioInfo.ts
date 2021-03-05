import { IApplicationSettings } from './applicationSettings';

export interface IPortfolioInfo {
    settings: IApplicationSettings;
    hrefGetActiveExhibits: string;
    hrefGetExhibit: string;
    hrefGetLeet: string;
}
