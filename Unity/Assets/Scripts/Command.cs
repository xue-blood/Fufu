using System;

public class Command {

    public Predicate<object> CanExecuteDelegate {
        get;
        set;
    }
    public Action<object> ExecuteDelegate {
        get;
        set;
    }

    public bool CanExecute ( object parameter ) {
        if (CanExecuteDelegate != null)
            return CanExecuteDelegate (parameter);
        return true; // if there is no can execute default to true
    }

    public void Execute ( object parameter ) {
        if (ExecuteDelegate != null)
            ExecuteDelegate (parameter);
    }
}