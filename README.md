# Simple-RUDP
I wanted to learn how does RUDP (Reliable UDP) work and started designing low level module for Unity (C#) networking system I develop.

# Target Features:
- Unreliable channel (pure UDP)
- Unreliable sequenced channel (some packets may be dropped, but will arrive in proper order
- Reliable unordered [semi-reliable] (all packets should arrive, but may be in invalid order)
- Reliable ordered (TCP simulation)

# Notes
- This thing is in WIP state.
- Do NOT use in production/real apps. This is made ONLY for learning proposes. Sometimes I do bad practises to simplify the code. Feel free to inspire yourself with this in your own library, but you USE it on your own responsibility. You have been warned.