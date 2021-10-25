# mFIT: A Bump-in-the-Wire Tool for Plug-and-Play Analysis of Rowhammer Susceptibility Factors

This repository captures the design and software for mFIT (the **m**emory **f**ault **i**njection **t**ool).

mFIT interposes between the CPU and a DDR4 DIMM and can programmatically suppress the REF command.
This allows researchers to study DRAM DIMMs in the absence of the refresh operation.

Please refer to the Tech Report [MSR-TR-2021-25](https://www.microsoft.com/en-us/research/) for a technical description of mFIT.


[![A picture of mFIT in a server](./hardware/binary/mfit-in-system.png)](https://www.microsoft.com/en-us/research/)

# Repository organization

1. [hardware](./hardware) contains the KiCAD schematics and PCB desings,
1. [firmware](./firmware) contains the code that runs on mFIT's control board,
1. [client](./client) contains the client code that interacts with mFIT's control board.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
