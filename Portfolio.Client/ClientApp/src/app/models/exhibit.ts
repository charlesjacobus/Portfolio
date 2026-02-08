import { IWork } from "./work";

export interface IExhibitSummary {
    id: number;
    name: string;
    description: string | null;
    descriptionFileName: string;
    anchor: string;
    promo: IWork | null;
    textIsDefault: boolean;
    textLabel: string | null;
    textRoute: string | null;
}

export interface IExhibit extends IExhibitSummary {
    works: Array<IWork>;
}
