EXTERNAL SelectChoice(index)
EXTERNAL AcceptQuest(q_id)
EXTERNAL AdvanceQuest(q_id)
EXTERNAL CompleteQuest(q_id)

VAR welcome_message = ""

VAR c0_label = ""
VAR c1_label = ""
VAR c2_label = ""
VAR c3_label = ""
VAR c4_label = ""
VAR c5_label = ""

=== NPC ===
{welcome_message} #speaker: trigger
* {c0_label != ""} [{c0_label}]
    ~ SelectChoice(0)
* {c1_label != ""} [{c1_label}]
    ~ SelectChoice(1)
* {c2_label != ""} [{c2_label}]
    ~ SelectChoice(2)
* {c3_label != ""} [{c3_label}]
    ~ SelectChoice(3)
* {c4_label != ""} [{c4_label}]
    ~ SelectChoice(4)
* {c5_label != ""} [{c5_label}]
    ~ SelectChoice(5)
- -> DONE
    
=== Quest_5713458344567873146_Offer ===
q_5713_offer_dialog_1 #speaker: Jhon // "Skeleton soldiers have been appearing near the village."

q_5713_offer_dialog_2 // "They're scaring away the merchants and travelers."

q_5713_offer_dialog_3 // "Could you defeat one of them for me?"

* [q_5713_offer_choice_yes] // "Yes"
    ~ AcceptQuest("5713458344567873146")
    <> q_5713_offer_accept // "Thank you! Please be careful out there."
    
* [q_5713_offer_choice_no] // "No"
    <> q_5713_offer_decline // "I understand. It's dangerous work."
    
- -> END

=== Quest_5713458344567873146_0_Progress ===
q_5713_progress_s0_dialog_1 #speaker: Jhon // "Have you defeated the skeleton soldier yet?"
-> END

=== Quest_5713458344567873146_0_Complete ===
q_5713_s0_complete_1 #speaker: Jhon // "You defeated one! The village will be safer now."
-> END

=== Quest_5713458344567873146_Complete ===
q_5713_complete_dialog_1 #speaker: Jhon // "The roads are safer thanks to you!"
~ CompleteQuest("5713458344567873146")
-> END

=== Quest_3555651423157362242_Offer ===
q_3555_offer_dialog_1 #speaker: Jhon // "Bob has been sick for days now."

q_3555_offer_dialog_2 // "Can you deliver this healing potion to him?"

* [q_3555_offer_choice_yes] // "Yes"
    ~ AcceptQuest("3555651423157362242")
    <> q_3555_offer_accept // "Thank you! He really needs this."
    
* [q_3555_offer_choice_no] // "No"
    <> q_3555_offer_decline // "Oh I see. I hope I can find someone else."
    
- -> END

=== Quest_3555651423157362242_0_Progress ===
q_3555_progress_s0_dialog_1 #speaker: Bob // "Did Jhon send you?"
-> END

=== Quest_3555651423157362242_Complete ===
q_3555_complete_dialog_1 #speaker: Bob // "This potion will help me recover. Thank you so much!"
~ CompleteQuest("3555651423157362242")
->END