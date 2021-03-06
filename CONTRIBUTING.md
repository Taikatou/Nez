Guidelines
==========

When contributing to Nez, please follow the coding guidelines set forth in the code. If you are using Xamarin there is a policy file (P31XamarinPolicy.mdpolicy.xml) in the root of the repository that you can use to format your code. It will work for *almost* all code except enum declarations (due to a bug in Xamarin). You can apply the policy by using the Project -> Apply Policy menu item.


Submitting Patches
==================

The process for making a pull request is generally as follows:

1. Make a feature branch in your own fork for the change.
2. Edit, build and test the feature.
3. Commit to your local repository.
4. Push the feature branch to your GitHub fork.
5. Create the pull request.

If you need to make changes to the pull request, simply repeat steps 2-4. Adding commits to that feature branch in your fork will automatically add the change to the pull request.

The majority of code in Nez is cross-platform and must build and behave correctly on all supported platforms.
