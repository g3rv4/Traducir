import axios from "axios";
import { Location } from "history";
import * as React from "react";
import history from "../../history";
import ISOStringSuggestion from "../../Models/SOStringSuggestion";
import IUserInfo from "../../Models/UserInfo";

export interface ISuggestionsHistoryProps {
    showErrorMessage: (messageOrCode: string | number) => void;
    currentUser?: IUserInfo;
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

    public async componentDidMount() {
        this.userChanged(this.props.location);
    }

    public componentWillReceiveProps(nextProps: ISuggestionsHistoryProps, context: any) {
        if (nextProps.location.pathname !== this.props.location.pathname) {
            this.userChanged(nextProps.location);
        }
    }

    public render(): React.ReactNode {
        if (!this.props.currentUser || !this.state.suggestions) {
            return null;
        }
        return this.state.suggestions.map(s => <div key={s.id}>{s.id}</div>);
    }

    public async userChanged(location: Location) {
        const userId = location.pathname.split("/").pop();
        try {
            const r = await axios.get<ISOStringSuggestion[]>(`/app/api/suggestions-by-user/${userId}`);
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
