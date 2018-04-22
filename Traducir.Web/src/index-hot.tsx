import * as React from "react";
import * as ReactDOM from "react-dom";
import { hot } from "react-hot-loader";
import { Router } from "react-router-dom";
import Traducir from "./App/Traducir";
import history from "./history";

const TraducirHot = hot(module)(Traducir);

ReactDOM.render(
  <Router history={history}>
    <TraducirHot />
  </Router>,
  document.getElementById("root")
);
