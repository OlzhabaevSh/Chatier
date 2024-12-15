import { Stack, IStackTokens, IStackStyles } from '@fluentui/react';
import './App.css';
import { AppNavBar } from './layout/appNavBar';
import { ChatDashboard } from './chat/chatDashboard';
import { useUser } from './hooks/useUser';
import { Redirect, Route, BrowserRouter as Router, Switch } from 'react-router-dom';
import { Login } from './layout/login';
import { initializeIcons } from '@fluentui/font-icons-mdl2';

const stackTokens: IStackTokens = { 
  childrenGap: 0
};
const stackStyles: Partial<IStackStyles> = {
  root: {
    margin: '0 auto',
    textAlign: 'center',
    color: '#605e5c',
    width: '100%',
    height: '100vh'
  },
};

initializeIcons();

export const App: React.FunctionComponent = () => {
  const [userName] = useUser();

  return (
    <Router>
      <Stack 
        horizontalAlign="baseline" 
        verticalAlign="baseline" 
        verticalFill 
        styles={stackStyles} 
        tokens={stackTokens}>
          <Stack.Item align="stretch">
            <AppNavBar title="Chatier" />
          </Stack.Item>
          <Stack.Item align="stretch" grow>
            <Switch>
              <Route path="/login" component={Login} />
              <Route path="/dashboard">
                {userName ? <ChatDashboard /> : <Redirect to="/login" />}
              </Route>
              <Redirect from="/" to="/dashboard" />
            </Switch>
          </Stack.Item>
      </Stack>
    </Router>
  );
};
