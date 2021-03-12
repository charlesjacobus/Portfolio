import { IWork } from "./work";

export interface IExhibitSummary {
    id: number;
    name: string;
    description: string;
    descriptionFileName: string;
    anchor: string;
    promo: IWork;
    textLabel: string;
    textRoute: string;
}

export interface IExhibit extends IExhibitSummary {
    works: Array<IWork>;
}
