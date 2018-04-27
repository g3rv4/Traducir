import { ReactChild, ReactFragment, ReactPortal } from "react";

// React.Component.render() should return NonUndefinedReactNode insetad of React.ReactNode since undefined is not allowed
// helper render methods can actually return React.ReactNode
type NonUndefinedReactNode = ReactChild | ReactFragment | ReactPortal | string | number | boolean | null;