import { IApplicationSettings } from './applicationSettings';

export interface IPortfolioInfo {
    settings: IApplicationSettings;
    hrefGetActiveExhibits: string;
    hrefGetActiveWritings: string;
    hrefGetExhibit: string;
    hrefGetLeet: string;
    hrefGetPhoto: string;
    hrefGetWriting: string;
}
