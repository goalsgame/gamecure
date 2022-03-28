scmVarsHolder = []

def setScmVars(scmVars) {
    scmVarsHolder[0] = scmVars
}

def getBranch() {
    return scmVarsHolder[0].GIT_BRANCH.replace("/", "-")
}

def getCommit() {
    return scmVarsHolder[0].GIT_COMMIT
}

def getCommitShort() {
    return getCommit().substring(0, 8)
}

// Needed to be able to import into Jenkinsfiles
return this
