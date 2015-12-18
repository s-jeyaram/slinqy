# What is Slinqy?
A tool for applications to automatically scale their queue resources at runtime based on demand.

[![Build status](https://ci.appveyor.com/api/projects/status/3msjix5fdfe5u5fs?svg=true)](https://ci.appveyor.com/project/rakutensf-malex/slinqy)
[![Coverage Status](https://coveralls.io/repos/stealthlab/slinqy/badge.svg?branch=master&service=github)](https://coveralls.io/github/stealthlab/slinqy?branch=master)

## Features
### Auto Expanding Storage Capacity

During normal operation, your application will process queue messages in a timely fashion.  
  
Unfortunately, issues can arise that prevent your back end from processing queue messages for prolonged periods of time.
Queues, high traffic queues in particular, can become full in such situations.  Either requiring frantic manual intervention or worse,
reaches full and the upstream users begin receiving errors...

Slinqy will automatically grow the storage capacity of your queue if utilization nears full so that you and your users never encounter queue full errors.