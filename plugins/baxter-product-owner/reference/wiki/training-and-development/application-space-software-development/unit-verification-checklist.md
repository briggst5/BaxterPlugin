# Training and Development/Application Space Software Development/Unit Verification Checklist

# Unit Verification Checklist

- Per [PLT1-811 - Identification of Categories of Defects](https://polarion.hrc.corp/polarion/redirect/project/flc.platform.01/workitem?id=PLT1-811) 
  - Arithmetic issues  
    - Divide by zero  
    - Numeric overflow/underflow 
    - Floating point rounding 
    - Improper range/bounds checking 
    - Off-by-one 
  - HW interactions 
    - EEPROM/NVRAM wear out 
    - CPU/HW faults 
    - Peripheral anomalies  
  - Timing 
    - Race conditions 
    - Missed timing deadlines  
  - Moding  
    - Abnormal termination  
    - Power loss/recovery 
    - Enter/exit low power modes  
  - Data Issues 
    - Data corruption  
    - Resource contention  
    - Errant pointers 
  - Data conversion errors  
    - Incorrect initialization  
    - Averaged data out of range  
    - Rollovers 
  - Interface Issues 
    - Failing to update display  
  - Misuse  
    - System overload 
    - SOUP failures 
  - Misc 
    - Memory/resource leaks 
    - Stack overflow  
    - Logic errors  
- [GQP-09-05](https://worksites.baxter.com/:b:/r/sites/TWP_PRD_DOCCLASS1/TCU_PRD_BaxDoc(I)/Baxter%20General%20Document/GQP-09-05/GQP-09-05,D/GQP-09-05.pdf?csf=1&web=1&e=RaiblH) 6.6.4 Unit Verification Criteria  
  - Proper event sequence 
  - Data and control flow 
  - Planned resource allocation  
  - Fault handling (error definition, isolation, and recovery)  
  - Initialization of variables  
  - Self-diagnostics  
  - Memory management and memory overflows 
  - Boundary conditions  
  - Code Review Checklist 

## Additional Code Guidelines 

### General 
- Code should not use recursion
- No experimental/opt-in APIs or libraries
- Design details should specify the design patterns used
### Kotlin 
- No thread usage in Kotlin without explanation
- Code should not use “wait” or “delay”
