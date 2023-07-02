using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dalamud;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace totalRoleplay;

[PluginInterface]
class QuestService : IServiceType
{
    public Dictionary<string, Model.Quest> Quests { get; init; }
    public List<Model.ActiveQuest> ActiveQuests { get; } = new List<Model.ActiveQuest>();

    public delegate void OnQuestComplete(string questId);
    public event OnQuestComplete? QuestComplete;

    public QuestService(DalamudPluginInterface pluginInterface)
    {
        string questJsonPath = Path.Join(pluginInterface.AssemblyLocation.Directory!.FullName, "quests.json");
        PluginLog.LogInformation(questJsonPath);
        string questJson = File.ReadAllText(questJsonPath);
        Quests = JsonSerializer.Deserialize<Dictionary<string, Model.Quest>>(
            questJson,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        )!;
    }

    public void BeginQuest(string questId)
    {
        ActiveQuests.Add(new Model.ActiveQuest
        {
            QuestId = questId,
            CurrentState = Quests[questId].InitialState
        });
    }

    public void TriggerCommand(string cmd) {
        foreach (var aq in ActiveQuests) {
            var quest = Quests[aq.QuestId];
            var state = quest.States[aq.CurrentState];
            foreach (var trigger in state.Triggers) {
                if (trigger.When.Command == cmd) {
                    ExecuteTriggerActions(aq.QuestId, trigger.Then);
                }
            }
        }
    }

    private void ExecuteTriggerActions(string questId, Model.QuestStateTriggerAction action) {
        if (action.GoToState != null) {
            ActiveQuests.FindAll(aq => aq.QuestId == questId).ForEach(aq => aq.CurrentState = action.GoToState);
        }
        if (action.FinishQuest) {
            if (QuestComplete != null) {
                QuestComplete(questId);
            }
            ActiveQuests.RemoveAll(aq => aq.QuestId == questId);
        }
    }
}