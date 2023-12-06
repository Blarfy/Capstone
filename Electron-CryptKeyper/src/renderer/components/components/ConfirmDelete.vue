<template>
    <div class="confirm-delete">
        <div class="confirm-delete__container">
        <div class="confirm-delete__header">
            <h2 class="confirm-delete__title">Are you sure you want to delete this item?</h2>
        </div>
        <div class="confirm-delete__body">
            <div class="confirm-delete__buttons">
            <button class="confirm-delete__button" @click="confirmDelete">Yes</button>
            <button class="confirm-delete__button" @click="cancelDelete">No</button>
            </div>
        </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'ConfirmDelete',
    methods: {
        confirmDelete() {
            this.$emit('confirm-delete');
        },
        cancelDelete() {
            this.$emit('cancel-delete');
        }
    }
}
</script>

<style scoped>
.confirm-delete {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    backdrop-filter: blur(5px);
    background-color: rgba(0, 0, 0, 0.5);
    z-index: 100;
}

.confirm-delete__container {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: auto;
    height: auto;
    background-color: #fff;
    border-radius: 5px;
    padding: 20px;
}

.confirm-delete__header {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-bottom: 20px;
}

.confirm-delete__title {
    font-size: 1.5rem;
    font-weight: bold;
}

.confirm-delete__body {
    display: flex;
    justify-content: center;
    align-items: center;
}

.confirm-delete__buttons {
    display: flex;
    justify-content:space-evenly;
    align-items: center;
}

.confirm-delete__button {
    padding: 10px 20px;
    border-radius: 5px;
    margin: 0 30px;
    border: none;
    background-color: #007bff;
    color: #fff;
    font-weight: bold;
    cursor: pointer;
}

.confirm-delete__button:hover {
    background-color: #0069d9;
}

.confirm-delete__button:active {
    background-color: #0062cc;
}

.confirm-delete__button:last-child {
    background-color: #dc3545;
}

.confirm-delete__button:last-child:hover {
    background-color: #c82333;
}

.confirm-delete__button:last-child:active {
    background-color: #bd2130;
}
</style>