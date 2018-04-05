import * as React from "react";
import * as ReactDOM from "react-dom";
import { Router } from 'react-router-dom'

import Traducir from "./App/Traducir";
import history from './history'

ReactDOM.render(
  <Router history={history}>
    <Traducir />
  </Router>,
  document.getElementById("root")
);