#iQu About


# Introduction #
iQu is processing engine for MS AD(DS) and LDAP attributes management. It's an extension to iMDirectory project.


# Related Projects #
  * [iMDirectory Project](https://code.google.com/p/imdirectory/)
  * [iCOR3 Project](https://code.google.com/p/icor3/)
  * [iAuthX Project](https://code.google.com/p/iauthx/)

# Use Cases - Examples #
## Migrate or merge AD DS environments ##
_After years of co-existence of several separated internal forests company decides to migrate all directories into one enterprise AD DS forest. iMDirectory as a complete directories repository is a source of migration information, which with relevant logic applied can be used to provision a new forest using legacy directory information._
_As several MS systems rely on AD DS data iMDirectory is an ideal solution for all types of migrations where AD DS was used for meta-data information. Used with custom migration framework eliminates need of purchasing products that can offer only migration functionality for very specific system configurations._
![https://imdirectory.googlecode.com/svn/wiki/Example2.gif](https://imdirectory.googlecode.com/svn/wiki/Example2.gif)
Figure 3. MS AD(DS) Migration - Example

## RBAC via ABAC ##
_Company decides to implement Role-based Access Control model across company internal systems. As LDAP or MS AD DS were used for years for Identity and Access Control there is a requirement to use the framework that can almost transparently integrate with exiting IAM infrastructure.
This component can be used to stream data out to LDAP or AD DS based on defined conditions. If one of the conditions is to allow VPN access to all HR and IT employees the RBAC role can be executed via Attribute-based Access Control. This approach would check specific attribute using custom filter, e.g. department LIKE ‘HR’ OR department LIKE ‘IT’ in order to decide upon VPN security entitlements._
_All of these examples require additional components, which are part of a different framework, although iMDirectory is a core part of each of these frameworks. iMDirectory can be easily integrated with any custom IAM framework._