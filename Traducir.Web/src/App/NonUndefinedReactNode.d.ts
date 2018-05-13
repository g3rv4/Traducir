import { ReactNode } from "react";

// React.Component.render() should return NonUndefinedReactNode insetad of React.ReactNode since undefined is not allowed
// helper render methods can actually return React.ReactNode
type NonUndefinedReactNode = Exclude<ReactNode, undefined>
