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
A. #speaker: Jhon

B.

C.
* [Yes]
    ~ AcceptQuest("5713458344567873146")
    Great!
* [No]
    Oh, ok then come back if change your mind
- -> END

=== Quest_5713458344567873146_0_Progress ===
I'm begging you. #speaker: Jhon
-> END

=== Quest_5713458344567873146_Complete ===
Thank you. #speaker: Jhon
~ CompleteQuest("5713458344567873146")
-> END

=== Quest_3555651423157362242_Offer ===
A. #speaker: Jhon

B.
* [Yes]
    ~ AcceptQuest("3555651423157362242")
    Great!
* [No]
    Oh, ok then come back if change your mind
- -> END

=== Quest_3555651423157362242_0_Progress ===
How can i help you? #speaker: Bob
-> END

=== Quest_3555651423157362242_Complete ===
Great! #speaker: Bob
~ CompleteQuest("3555651423157362242")
->END

