import axios from "axios";
import { Location } from "history";
import * as React from "react";
import history from "../../history";
import IConfig from "../../Models/Config";
import ISOStringSuggestion, { StringSuggestionState, suggestionStateToString } from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";
import { NonUndefinedReactNode } from "../NonUndefinedReactNode";
import SuggestionHistoryFilters from "./SuggestionHistoryFilters";
import SuggestionHistoryTable from "./SuggestionsHistoryTable";

export interface ISuggestionsHistoryProps {
    showErrorMessage: (messageOrCode: string | number) => void;
    currentUser?: IUserInfo;
    config: IConfig;
    location: Location;
}

interface ISuggestionsHistoryState {
    suggestions?: ISOStringSuggestion[];
}

export default class SuggestionsHistory extends React.Component<ISuggestionsHistoryProps, ISuggestionsHistoryState> {
    constructor(props: ISuggestionsHistoryProps) {
        super(props);

        this.state = {
            suggestions: undefined
        };
    }

    public componentDidMount(): void {
        this.userChanged(this.props.location);
    }

    public componentWillReceiveProps(nextProps: ISuggestionsHistoryProps, context: any): void {
        if ((nextProps.location.pathname !== this.props.location.pathname)
            || (nextProps.location.search !== this.props.location.search)) {
            this.userChanged(nextProps.location);
        }
    }

    public render(): NonUndefinedReactNode {
        if (!this.props.currentUser || !this.state.suggestions) {
            return null;
        }
        return <div>
            <SuggestionHistoryFilters
                userid={this.props.currentUser.id}
            />
            <SuggestionHistoryTable
                suggestions={this.state.suggestions}
                config={this.props.config}
            />
        </div>;
    }

    public async userChanged(location: Location): Promise<void> {
        const userId = location.pathname.split("/").pop();
        const stateId = location.search.substring(location.search.lastIndexOf("=") + 1);
        try {
            const r = await axios.get<ISOStringSuggestion[]>(`/app/api/suggestions-by-user/${userId}`, {params: {stateId}});
            this.setState({
                suggestions: r.data
            });
        } catch (e) {
            if (e.response.status === 401) {
                history.push("/");
            } else {
                this.props.showErrorMessage(e.response.status);
            }
        }
    }
}
