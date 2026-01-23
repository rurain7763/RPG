using Ink.Runtime;
using System;
using System.Collections.Generic;

public class Dialogue
{
    private Story story;
    public DialogueSpeaker triggerSpeaker;
    private Dictionary<string, DialogueVariable> globalVariables = new();

    internal Story Story => story;

    public DialogueSpeaker TriggerSpeaker => triggerSpeaker;

    public bool CanContinue => story.canContinue;
    public bool HasChoices => story.currentChoices.Count > 0;
    public List<DialogueChoice> CurrentChoices
    {
        get
        {
            List<DialogueChoice> choices = new List<DialogueChoice>();
            foreach (Choice choice in story.currentChoices)
            {
                choices.Add(new DialogueChoice(choice));
            }

            return choices;
        }
    }

    public bool HasTags => story.currentTags.Count > 0;
    public List<DialogueTag> CurrentTags
    {
        get
        {
            List<DialogueTag> tags = new List<DialogueTag>();
            foreach (string tag in story.currentTags)
            {
                tags.Add(new DialogueTag(tag));
            }
            return tags;
        }
    }

    public string CurrentText => story.currentText;

    private bool isExecuteExternalFunction;
    private Queue<IDialoguePendingTask> pendingTasks = new();

    internal Dialogue(Story story, DialogueSpeaker triggerSpeaker)
    {
        this.story = story;
        this.triggerSpeaker = triggerSpeaker;

        foreach (var variableName in story.variablesState)
        {
            var obj = story.variablesState.GetVariableWithName(variableName);
            var variable = new DialogueVariable(this, variableName, obj);
            globalVariables.Add(variableName, variable);
        }

        story.variablesState.variableChangedEvent += HandleVariableChanged;
    }

    private void HandleVariableChanged(string variableName, Ink.Runtime.Object newValue)
    {
        if (globalVariables.TryGetValue(variableName, out var dialogueVariable))
        {
            dialogueVariable.OnVariableChanged(newValue);
        }
    }

    public void ChoosePath(string path, params object[] arguments)
    {
        if (!isExecuteExternalFunction)
        {
            story.ChoosePathString(path, arguments: arguments);
        }
        else
        {
            pendingTasks.Enqueue(new PathChangeTask(path, arguments));
        }
    }

    public void Reset()
    {
        story.ResetState();
    }

    public void Continue()
    {
        if (!isExecuteExternalFunction)
        {
            story.Continue();
        }
        else
        {
            pendingTasks.Enqueue(new ContinueTask());
        }
    }

    public void ChooseChoiceIndex(int index)
    {
        story.ChooseChoiceIndex(index);
    }

    public void BindExternalFunction(string functionName, Action function)
    {
        story.BindExternalFunction(functionName, () =>
        {
            isExecuteExternalFunction = true;
            function();
            isExecuteExternalFunction = false;
        });
    }

    public void BindExternalFunction<T1>(string functionName, Action<T1> function)
    {
        story.BindExternalFunction(functionName, (T1 arg1) =>
        {
            isExecuteExternalFunction = true;
            function(arg1);
            isExecuteExternalFunction = false;
        });
    }

    public void BindExternalFunction<T1, T2>(string functionName, Action<T1, T2> function)
    {
        story.BindExternalFunction(functionName, (T1 arg1, T2 arg2) =>
        {
            isExecuteExternalFunction = true;
            function(arg1, arg2);
            isExecuteExternalFunction = false;
        });
    }

    public void UnbindExternalFunction(string functionName)
    {
        story.UnbindExternalFunction(functionName);
    }

    public DialogueVariable GetVariable(string name)
    {
        if (globalVariables.TryGetValue(name, out var variable))
        {
            return variable;
        }
        return null;
    }

    public void SetVariable(string name, object value)
    {
        if (globalVariables.TryGetValue(name, out var variable))
        {
            variable.SetValue(value);
        }
        else
        {
            throw new ArgumentException($"Variable '{name}' not found.");
        }
    }

    public void ExecutePendingTasks()
    {
        while (pendingTasks.Count > 0)
        {
            var task = pendingTasks.Dequeue();
            task.Execute(story);
        }
    }
}

internal interface IDialoguePendingTask
{
    void Execute(Story story);
}

internal class PathChangeTask : IDialoguePendingTask
{
    public readonly string Path;
    public readonly object[] Arguments;

    public PathChangeTask(string path, params object[] arguments)
    {
        Path = path;
        Arguments = arguments;
    }

    public void Execute(Story story)
    {
        story.ChoosePathString(Path);
    }
}

internal class ContinueTask : IDialoguePendingTask
{
    public void Execute(Story story)
    {
        story.Continue();
    }
}

public class DialogueChoice
{
    private Choice choice;

    public string Text => choice.text;

    internal DialogueChoice(Choice choice)
    {
        this.choice = choice;
    }
}

public class DialogueVariable
{
    private Dialogue dialogue;

    private string name;
    private Ink.Runtime.Object obj;

    public string Name => name;

    internal DialogueVariable(Dialogue dialogue, string name, Ink.Runtime.Object obj)
    {
        this.dialogue = dialogue;
        this.name = name;
        this.obj = obj;
    }

    internal void OnVariableChanged(Ink.Runtime.Object newValue)
    {
        obj = newValue;
    }

    public void SetValue(object value)
    {
        if (value is int intValue)
        {
            obj = new IntValue(intValue);
        }
        else if (value is float floatValue)
        {
            obj = new FloatValue(floatValue);
        }
        else if (value is bool boolValue)
        {
            obj = new BoolValue(boolValue);
        }
        else if (value is string stringValue)
        {
            obj = new StringValue(stringValue);
        }
        else
        {
            throw new ArgumentException("Unsupported variable type");
        }

        dialogue.Story.variablesState.SetGlobal(name, obj);
    }

    public T GetValue<T>()
    {
        if (typeof(T) == typeof(int) && obj is IntValue intValue)
        {
            return (T)(object)intValue.value;
        }
        else if (typeof(T) == typeof(float) && obj is FloatValue floatValue)
        {
            return (T)(object)floatValue.value;
        }
        else if (typeof(T) == typeof(bool) && obj is BoolValue boolValue)
        {
            return (T)(object)boolValue.value;
        }
        else if (typeof(T) == typeof(string) && obj is StringValue stringValue)
        {
            return (T)(object)stringValue.value;
        }
        else
        {
            throw new ArgumentException("Unsupported variable type or mismatched type");
        }
    }
}

public class DialogueTag
{
    public readonly string Key;
    public readonly string Value;

    internal DialogueTag(string tag)
    {
        string[] splits = tag.Split(new char[] { ':' }, 2);

        splits[0] = splits[0].Trim();
        Key = splits[0];

        splits[1] = splits[1].Trim();
        Value = splits[1].Trim();
    }

    public T GetValue<T>()
    {
        if (typeof(T) == typeof(int) && int.TryParse(Value, out int intValue))
        {
            return (T)(object)intValue;
        }
        else if (typeof(T) == typeof(float) && float.TryParse(Value, out float floatValue))
        {
            return (T)(object)floatValue;
        }
        else if (typeof(T) == typeof(bool) && bool.TryParse(Value, out bool boolValue))
        {
            return (T)(object)boolValue;
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)Value;
        }
        else
        {
            throw new ArgumentException("Unsupported variable type or mismatched type");
        }
    }
}