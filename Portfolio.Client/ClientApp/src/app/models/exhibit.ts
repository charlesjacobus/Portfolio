import { ISpecial } from "./special";
import { IWork } from "./work";

export interface IExhibitSummary {
    id: number;
    name: string;
    description: string;
    descriptionFileName: string;
    anchor: string;
    promo: IWork;
    special: ISpecial;
}

export interface IExhibit extends IExhibitSummary {
    works: Array<IWork>;
}
