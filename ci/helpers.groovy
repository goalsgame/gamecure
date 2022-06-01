scmVarsHolder = []

def setScmVars(scmVars) {
	scmVarsHolder = scmVars
}

def getBranch() {
	return scmVarsHolder.GIT_BRANCH.replace("/", "-")
}

def getCommit() {
	return scmVarsHolder.GIT_COMMIT
}

def getshortCommit() {
	return getCommit().substring(0, 7)
}

def getBuildId() {
	def timestamp = new Date(currentBuild.timeInMillis).format("yyMMdd-HHmm")
	def branch = scmVarsHolder.GIT_BRANCH.replace("/", "-")
	def shortHash = getshortCommit()
	return "${branch}_${timestamp}_${shortHash}"
}

def getAuthor() {
	if (isUnix()) 
		return sh(returnStdout: true, script: "git log -1 --pretty=format:'%ae'").trim()
	else
		return bat(returnStdout: true, script: "git log -1 --pretty=format:'%%ae'").trim().readLines().drop(1).join(" ")
}

def buildStarted( String name) {
    def map = getMap(name)
	argoWorkflowHelper.triggerWorkflow("build-started-workflow", scmVarsHolder.GIT_BRANCH, map)
}

def buildFailed( String name) {
	def map = getMap(name)
	argoWorkflowHelper.triggerWorkflow("build-failed-workflow", scmVarsHolder.GIT_BRANCH, map)
}

def buildSuccess(String name, List services = null, String namespace = "", String platform = "", String channel = "") {
	def map = getMap(name, services, namespace)
	argoWorkflowHelper.triggerWorkflow("build-success-workflow-template", scmVarsHolder.GIT_BRANCH, map)
}

def getbuildDescription(String name, List services = null, String namespace = ""){
	return name
}
// maps key = variable name and value = data in variable (to get both name/value from the variable)
def getMap(String name, List services = null, String namespace = ""){
	def channel="",platform="" //being used in argo workflow so pass empty values
	def namestr = name.split(' ');
	return [name: namestr[0], channel: channel, platform: platform,"buildDescription": getbuildDescription(name, services, namespace),
		"version": getBuildId(), "branch": scmVarsHolder.GIT_BRANCH, "commit": getshortCommit(), "author": getAuthor(), "publishEvent": "false"]
}
// Needed to be able to import into Jenkinsfiles
return this
